using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Touching_Grass.Models;

namespace Touching_Grass.Services;

public sealed class OpenMeteoService(
    HttpClient httpClient,
    ILogger<OpenMeteoService> logger)
{
    public async Task<WeatherObservation> GetEvapotranspirationAsync(
        ZipLocation location,
        WeatherPeriod period,
        CancellationToken cancellationToken = default)
    {
        var latitude = location.Latitude.ToString(CultureInfo.InvariantCulture);
        var longitude = location.Longitude.ToString(CultureInfo.InvariantCulture);
        var timeZone = Uri.EscapeDataString(location.TimeZoneId);
        var requestUri =
            $"v1/forecast?latitude={latitude}&longitude={longitude}" +
            $"&daily=et0_fao_evapotranspiration&past_days={WeatherPeriod.DayCount}&forecast_days=0" +
            $"&timezone={timeZone}";
        var fullRequestUrl = new Uri(httpClient.BaseAddress!, requestUri).ToString();

        try
        {
            logger.LogInformation("Open-Meteo request URL: {RequestUrl}", fullRequestUrl);
            using var response = await httpClient.GetAsync(
                requestUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new WeatherDataUnavailableException();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);

            var totalMillimeters = SumRequestedDays(document.RootElement, period);
            return new WeatherObservation(
                WeatherMetric.Evapotranspiration,
                totalMillimeters / 25.4m,
                period.StartDate,
                period.EndDate,
                location.TimeZoneId,
                "Open-Meteo");
        }
        catch (WeatherDataUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new WeatherDataUnavailableException();
        }
        catch (HttpRequestException)
        {
            throw new WeatherDataUnavailableException();
        }
        catch (JsonException)
        {
            throw new WeatherDataUnavailableException();
        }
        catch (IOException)
        {
            throw new WeatherDataUnavailableException();
        }
        catch (OverflowException)
        {
            throw new WeatherDataUnavailableException();
        }
    }

    private static decimal SumRequestedDays(JsonElement root, WeatherPeriod period)
    {
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("daily", out var daily) ||
            daily.ValueKind != JsonValueKind.Object ||
            !daily.TryGetProperty("time", out var dates) ||
            !daily.TryGetProperty("et0_fao_evapotranspiration", out var values) ||
            dates.ValueKind != JsonValueKind.Array ||
            values.ValueKind != JsonValueKind.Array ||
            dates.GetArrayLength() != values.GetArrayLength())
        {
            throw new WeatherDataUnavailableException();
        }

        var totalsByDate = new Dictionary<DateOnly, decimal>();
        for (var index = 0; index < dates.GetArrayLength(); index++)
        {
            if (!dates[index].TryGetDateOnly(out var date))
            {
                throw new WeatherDataUnavailableException();
            }

            if (date < period.StartDate || date > period.EndDate)
            {
                continue;
            }

            if (values[index].ValueKind != JsonValueKind.Number ||
                !values[index].TryGetDecimal(out var value) ||
                value < 0 ||
                !totalsByDate.TryAdd(date, value))
            {
                throw new WeatherDataUnavailableException();
            }
        }

        if (totalsByDate.Count != WeatherPeriod.DayCount)
        {
            throw new WeatherDataUnavailableException();
        }

        return totalsByDate.Values.Sum();
    }
}

internal static class JsonElementExtensions
{
    public static bool TryGetDateOnly(this JsonElement element, out DateOnly date)
    {
        date = default;
        return element.ValueKind == JsonValueKind.String &&
               DateOnly.TryParseExact(
                   element.GetString(),
                   "yyyy-MM-dd",
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
                   out date);
    }
}
