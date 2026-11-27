using System.Globalization;
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
        var location = new ZipLocation
        {
            ZipCode = "95814",
            TimeZoneId = "America/Los_Angeles"
        };
        var plant = new Plant
        {
            CommonName = "Toyon",
            ScientificName = "Heteromeles arbutifolia",
            WaterFactor = 0.40m
        };

        var recommendation = await new WateringCalculatorService().CalculateAsync(location, plant);
        var status = _service.Create(recommendation.NetNeedInches);

        Assert.Equal(0.14m, recommendation.NetNeedInches);
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
