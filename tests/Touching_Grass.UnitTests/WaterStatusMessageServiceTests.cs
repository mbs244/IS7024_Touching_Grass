using System.Globalization;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class WaterStatusMessageServiceTests
{
    private readonly WaterStatusMessageService _service = new();

    [Theory]
    [InlineData(0.00, "No extra watering needed this week.")]
    [InlineData(0.004, "No extra watering needed this week.")]
    [InlineData(0.14, "Water lightly this week: about 0.14 in.")]
    [InlineData(0.25, "Water about 0.25 in this week.")]
    [InlineData(0.80, "Water about 0.80 in this week.")]
    public void CreateReturnsExpectedStatus(decimal netNeedInches, string expectedMessage)
    {
        var result = _service.Create(netNeedInches);

        Assert.Equal(expectedMessage, result.Message);
    }

    [Fact]
    public void CreateRejectsNegativeNeed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Create(-0.01m));
    }

    [Fact]
    public async Task CreateAgreesWithWateringCalculatorOutput()
    {
        var period = WeatherApiTestHelpers.Period;
        using var openMeteoClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(WeatherApiTestHelpers.OpenMeteoResponse(period, 2.1771428571428571428571428571m))
            }))
        {
            BaseAddress = new Uri("https://open-meteo.test/")
        };
        using var visualCrossingClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(WeatherApiTestHelpers.VisualCrossingResponse(period, 0.0142857142857142857142857143m))
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
        var calculator = new WateringCalculatorService(
            new OpenMeteoService(openMeteoClient, NullLogger<OpenMeteoService>.Instance),
            new VisualCrossingService(visualCrossingClient, configuration));

        var plant = new Plant
        {
            CommonName = "Toyon",
            ScientificName = "Heteromeles arbutifolia",
            WaterFactor = 0.40m
        };

        var recommendation = await calculator.CalculateAsync(WeatherApiTestHelpers.Location, plant);
        var status = _service.Create(recommendation.NetNeedInches);

        Assert.Equal(0.14m, decimal.Round(recommendation.NetNeedInches, 2, MidpointRounding.AwayFromZero));
        Assert.Equal("Water lightly this week: about 0.14 in.", status.Message);
    }

    [Theory]
    [InlineData(0.005, "Water lightly this week: about 0.01 in.")]
    [InlineData(0.244, "Water lightly this week: about 0.24 in.")]
    [InlineData(0.245, "Water about 0.25 in this week.")]
    public void CreateRoundsAwayFromZeroBeforeSelectingMessage(decimal netNeedInches, string expectedMessage)
    {
        var result = _service.Create(netNeedInches);

        Assert.Equal(expectedMessage, result.Message);
    }

    [Fact]
    public void CreateFormatsAmountsInvariantly()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var result = _service.Create(0.14m);

            Assert.Equal("Water lightly this week: about 0.14 in.", result.Message);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
