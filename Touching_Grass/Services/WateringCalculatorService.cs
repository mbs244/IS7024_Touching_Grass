using Touching_Grass.Models;

namespace Touching_Grass.Services;

public sealed class WateringCalculatorService : IWateringCalculatorService
{
    public const decimal MockEvapotranspirationInches = 0.60m;
    public const decimal MockRainfallInches = 0.10m;

    public Task<WaterRecommendation> CalculateAsync(
        ZipLocation location,
        Plant plant,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var grossNeed = MockEvapotranspirationInches * plant.WaterFactor;
        var netNeed = Math.Max(0m, grossNeed - MockRainfallInches);
        var explanation =
            $"{MockEvapotranspirationInches:0.00} in ET × {plant.WaterFactor:0.00} plant factor " +
            $"− {MockRainfallInches:0.00} in rainfall = {netNeed:0.00} in net watering need.";

        return Task.FromResult(new WaterRecommendation
        {
            ZipCode = location.ZipCode,
            PlantDisplayName = $"{plant.CommonName} ({plant.ScientificName})",
            EvapotranspirationInches = MockEvapotranspirationInches,
            RainfallInches = MockRainfallInches,
            PlantFactor = plant.WaterFactor,
            GrossNeedInches = grossNeed,
            NetNeedInches = netNeed,
            Explanation = explanation
        });
    }
}
