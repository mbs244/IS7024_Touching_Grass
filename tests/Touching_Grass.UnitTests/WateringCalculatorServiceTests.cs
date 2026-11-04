using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class WateringCalculatorServiceTests
{
    private static readonly ZipLocation Location = new()
    {
        ZipCode = "95814",
        TimeZoneId = "America/Los_Angeles"
    };

    [Fact]
    public async Task CalculatesNetNeedUsingMockWeatherValues()
    {
        var result = await new WateringCalculatorService().CalculateAsync(Location, new Plant
        {
            CommonName = "Toyon",
            ScientificName = "Heteromeles arbutifolia",
            WaterFactor = 0.40m
        });

        Assert.Equal(0.60m, result.EvapotranspirationInches);
        Assert.Equal(0.10m, result.RainfallInches);
        Assert.Equal(0.24m, result.GrossNeedInches);
        Assert.Equal(0.14m, result.NetNeedInches);
        Assert.Contains("0.60 in ET", result.Explanation);
    }

    [Fact]
    public async Task ClampsRainfallOffsetToZero()
    {
        var result = await new WateringCalculatorService().CalculateAsync(Location, new Plant
        {
            CommonName = "Low Water Plant",
            ScientificName = "Low water plant",
            WaterFactor = 0m
        });

        Assert.Equal(0m, result.NetNeedInches);
    }

    [Fact]
    public async Task HonorsCancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            new WateringCalculatorService().CalculateAsync(Location, new Plant { WaterFactor = 0.40m }, cancellationTokenSource.Token));
    }
}
