using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class WeatherApiServiceTests
{
    [Fact]
    public async Task OpenMeteoSumsSevenDaysAndConvertsMillimetersToInches()
    {
        Uri? requestUri = null;
        var period = WeatherApiTestHelpers.Period;
        using var client = new HttpClient(new FakeHttpMessageHandler(request =>
        {
            requestUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(WeatherApiTestHelpers.OpenMeteoResponse(period, 25.4m))
            };
        }))
        {
            BaseAddress = new Uri("https://api.open-meteo.com/")
        };

        var observation = await new OpenMeteoService(client, NullLogger<OpenMeteoService>.Instance)
            .GetEvapotranspirationAsync(
            WeatherApiTestHelpers.Location,
            period);

        Assert.Equal(7m, observation.TotalInches);
        Assert.Equal(WeatherMetric.Evapotranspiration, observation.Metric);
        Assert.Equal(period.StartDate, observation.PeriodStart);
        Assert.Equal(period.EndDate, observation.PeriodEnd);
        Assert.NotNull(requestUri);
        Assert.Contains("daily=et0_fao_evapotranspiration", requestUri!.Query);
        Assert.Contains("past_days=7", requestUri.Query);
        Assert.Contains("forecast_days=0", requestUri.Query);
        Assert.Contains("timezone=America%2FLos_Angeles", requestUri.Query);
    }

    [Fact]
    public async Task VisualCrossingSumsSevenDailyRainfallValuesAndRequestsInches()
    {
        Uri? requestUri = null;
        var period = WeatherApiTestHelpers.Period;
        using var client = new HttpClient(new FakeHttpMessageHandler(request =>
        {
            requestUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(WeatherApiTestHelpers.VisualCrossingResponse(period, 0.1m))
            };
        }))
        {
            BaseAddress = new Uri("https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/")
        };
        var configuration = ConfigurationWithKey("unit-test-key");

        var observation = await new VisualCrossingService(client, configuration)
            .GetRainfallAsync(WeatherApiTestHelpers.Location, period);

        Assert.Equal(0.7m, observation.TotalInches);
        Assert.Equal(WeatherMetric.Rainfall, observation.Metric);
        Assert.NotNull(requestUri);
        Assert.Contains(
            $"timeline/38.5816,-121.4944/{period.StartDate:yyyy-MM-dd}/{period.EndDate:yyyy-MM-dd}",
            requestUri!.AbsoluteUri);
        Assert.Contains("unitGroup=us", requestUri.Query);
        Assert.Contains("include=days", requestUri.Query);
        Assert.Contains("elements=datetime,precip", requestUri.Query);
        Assert.Contains("key=unit-test-key", requestUri.Query);
    }

    [Fact]
    public async Task VisualCrossingMissingKeyReturnsSanitizedFailureWithoutSendingRequest()
    {
        using var client = new HttpClient(new FakeHttpMessageHandler(_ =>
            throw new InvalidOperationException("Request should not be sent.")))
        {
            BaseAddress = new Uri("https://weather.visualcrossing.com/")
        };
        var service = new VisualCrossingService(client, new ConfigurationBuilder().Build());

        var exception = await Assert.ThrowsAsync<WeatherDataUnavailableException>(() =>
            service.GetRainfallAsync(WeatherApiTestHelpers.Location, WeatherApiTestHelpers.Period));

        Assert.Equal(WeatherDataUnavailableException.UserMessage, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProviderHttpFailuresReturnSanitizedFailure(bool useVisualCrossing)
    {
        using var client = new HttpClient(new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("sensitive provider response")
            }))
        {
            BaseAddress = new Uri(useVisualCrossing
                ? "https://weather.visualcrossing.com/"
                : "https://api.open-meteo.com/")
        };
        var configuration = ConfigurationWithKey("secret-test-key");

        var exception = await Assert.ThrowsAsync<WeatherDataUnavailableException>(() =>
            useVisualCrossing
                ? new VisualCrossingService(client, configuration)
                    .GetRainfallAsync(WeatherApiTestHelpers.Location, WeatherApiTestHelpers.Period)
                : new OpenMeteoService(client, NullLogger<OpenMeteoService>.Instance)
                    .GetEvapotranspirationAsync(WeatherApiTestHelpers.Location, WeatherApiTestHelpers.Period));

        Assert.Equal(WeatherDataUnavailableException.UserMessage, exception.Message);
        Assert.DoesNotContain("sensitive provider response", exception.Message);
        Assert.DoesNotContain("secret-test-key", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProviderTimeoutsReturnSanitizedFailure(bool useVisualCrossing)
    {
        using var client = new HttpClient(new FakeHttpMessageHandler(_ =>
            throw new TaskCanceledException()))
        {
            BaseAddress = new Uri(useVisualCrossing
                ? "https://weather.visualcrossing.com/"
                : "https://api.open-meteo.com/")
        };
        var configuration = ConfigurationWithKey("secret-test-key");

        var exception = await Assert.ThrowsAsync<WeatherDataUnavailableException>(() =>
            useVisualCrossing
                ? new VisualCrossingService(client, configuration)
                    .GetRainfallAsync(WeatherApiTestHelpers.Location, WeatherApiTestHelpers.Period)
                : new OpenMeteoService(client, NullLogger<OpenMeteoService>.Instance)
                    .GetEvapotranspirationAsync(WeatherApiTestHelpers.Location, WeatherApiTestHelpers.Period));

        Assert.Equal(WeatherDataUnavailableException.UserMessage, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task OpenMeteoMissingDailyValueReturnsUnavailableFailure()
    {
        using var client = new HttpClient(new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"daily":{"time":["2026-01-01"],"et0_fao_evapotranspiration":[null]}}""")
            }))
        {
            BaseAddress = new Uri("https://api.open-meteo.com/")
        };

        var exception = await Assert.ThrowsAsync<WeatherDataUnavailableException>(() =>
            new OpenMeteoService(client, NullLogger<OpenMeteoService>.Instance)
                .GetEvapotranspirationAsync(
                WeatherApiTestHelpers.Location,
                WeatherApiTestHelpers.Period));

        Assert.Equal(WeatherDataUnavailableException.UserMessage, exception.Message);
    }

    private static IConfiguration ConfigurationWithKey(string apiKey) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VisualCrossing:ApiKey"] = apiKey
            })
            .Build();
}
