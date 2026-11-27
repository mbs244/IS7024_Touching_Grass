using System.Globalization;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class WateringCalculatorServiceTests
{
    [Fact]
    public async Task CalculatesRecommendationUsingSevenDayProviderTotalsInvariantly()
    {
        var period = WeatherApiTestHelpers.Period;
        using var openMeteoClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(WeatherApiTestHelpers.OpenMeteoResponse(period, 25.4m))
            }))
        {
            BaseAddress = new Uri("https://open-meteo.test/")
        };
        using var visualCrossingClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(WeatherApiTestHelpers.VisualCrossingResponse(period, 0.1m))
            }))
        {
            BaseAddress = new Uri("https://visual-crossing.test/")
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VisualCrossing:ApiKey"] = "test-key"
            })
            .Build();
        var service = new WateringCalculatorService(
            new OpenMeteoService(openMeteoClient, NullLogger<OpenMeteoService>.Instance),
            new VisualCrossingService(visualCrossingClient, configuration));
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var result = await service.CalculateAsync(WeatherApiTestHelpers.Location, new Plant
            {
                CommonName = "Toyon",
                ScientificName = "Heteromeles arbutifolia",
                WaterFactor = 0.40m
            });

            Assert.Equal(7m, result.EvapotranspirationInches);
            Assert.Equal(0.7m, result.RainfallInches);
            Assert.Equal(2.8m, result.GrossNeedInches);
            Assert.Equal(2.1m, result.NetNeedInches);
            Assert.Contains("7.00 in ET", result.Explanation);
            Assert.Contains("0.40 plant factor", result.Explanation);
            Assert.Contains("0.70 in rainfall", result.Explanation);
            Assert.Contains("2.10 in net watering need", result.Explanation);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task ClampsRainfallOffsetToZero()
    {
        var period = WeatherApiTestHelpers.Period;
        using var openMeteoClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(WeatherApiTestHelpers.OpenMeteoResponse(period, 0m))
            }))
        {
            BaseAddress = new Uri("https://open-meteo.test/")
        };
        using var visualCrossingClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(WeatherApiTestHelpers.VisualCrossingResponse(period, 1m))
            }))
        {
            BaseAddress = new Uri("https://visual-crossing.test/")
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VisualCrossing:ApiKey"] = "test-key"
            })
            .Build();
        var service = new WateringCalculatorService(
            new OpenMeteoService(openMeteoClient, NullLogger<OpenMeteoService>.Instance),
            new VisualCrossingService(visualCrossingClient, configuration));

        var result = await service.CalculateAsync(
            WeatherApiTestHelpers.Location,
            new Plant { WaterFactor = 0m });

        Assert.Equal(0m, result.NetNeedInches);
    }

    [Fact]
    public async Task HonorsCancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var service = new WateringCalculatorService(
            new OpenMeteoService(
                new HttpClient(new FakeHttpMessageHandler(_ =>
                    throw new InvalidOperationException("Request should not be sent."))),
                NullLogger<OpenMeteoService>.Instance),
            new VisualCrossingService(
                new HttpClient(new FakeHttpMessageHandler(_ =>
                    throw new InvalidOperationException("Request should not be sent."))),
                new ConfigurationBuilder().Build()));

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.CalculateAsync(
                WeatherApiTestHelpers.Location,
                new Plant { WaterFactor = 0.40m },
                cancellationTokenSource.Token));
    }
}
