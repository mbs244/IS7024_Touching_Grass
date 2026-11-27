using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Touching_Grass.Models;

namespace Touching_Grass.Services;

public sealed class VisualCrossingService(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<WeatherObservation> GetRainfallAsync(
        ZipLocation location,
        WeatherPeriod period,
        CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["VisualCrossing:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new WeatherDataUnavailableException();
        }

        var latitude = location.Latitude.ToString(CultureInfo.InvariantCulture);
        var longitude = location.Longitude.ToString(CultureInfo.InvariantCulture);
        var encodedKey = Uri.EscapeDataString(apiKey);
        var requestUri =
            $"timeline/{latitude},{longitude}/" +
            $"{period.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}/" +
            $"{period.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}" +
            $"?unitGroup=us&include=days&elements=datetime,precip&key={encodedKey}";

        try
        {
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

            var totalInches = SumRequestedDays(document.RootElement, period);
            return new WeatherObservation(
                WeatherMetric.Rainfall,
                totalInches,
                period.StartDate,
                period.EndDate,
                location.TimeZoneId,
                "Visual Crossing");
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
            !root.TryGetProperty("days", out var days) ||
            days.ValueKind != JsonValueKind.Array)
        {
            throw new WeatherDataUnavailableException();
        }

        var totalsByDate = new Dictionary<DateOnly, decimal>();
        foreach (var day in days.EnumerateArray())
        {
            if (day.ValueKind != JsonValueKind.Object ||
                !day.TryGetProperty("datetime", out var dateElement) ||
                !dateElement.TryGetDateOnly(out var date) ||
                date < period.StartDate ||
                date > period.EndDate ||
                !day.TryGetProperty("precip", out var precipitation) ||
                precipitation.ValueKind != JsonValueKind.Number ||
                !precipitation.TryGetDecimal(out var value) ||
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
