using System.Globalization;
using Touching_Grass.Models;

namespace Touching_Grass.Services;

public sealed class WateringCalculatorService(
    OpenMeteoService openMeteoService,
    VisualCrossingService visualCrossingService) : IWateringCalculatorService
{
    public async Task<WaterRecommendation> CalculateAsync(
        ZipLocation location,
        Plant plant,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var period = WeatherPeriod.PreviousSevenCompleteDays(location);
        var evapotranspirationTask = openMeteoService.GetEvapotranspirationAsync(
            location,
            period,
            cancellationToken);
        var rainfallTask = visualCrossingService.GetRainfallAsync(
            location,
            period,
            cancellationToken);
        await Task.WhenAll(evapotranspirationTask, rainfallTask);

        var evapotranspirationInches = (await evapotranspirationTask).TotalInches;
        var rainfallInches = (await rainfallTask).TotalInches;
        var grossNeed = evapotranspirationInches * plant.WaterFactor;
        var netNeed = Math.Max(0m, grossNeed - rainfallInches);
        var explanation = string.Format(
            CultureInfo.InvariantCulture,
            "{0:0.00} in ET × {1:0.00} plant factor − {2:0.00} in rainfall = {3:0.00} in net watering need.",
            evapotranspirationInches,
            plant.WaterFactor,
            rainfallInches,
            netNeed);

        return new WaterRecommendation
        {
            ZipCode = location.ZipCode,
            PlantDisplayName = $"{plant.CommonName} ({plant.ScientificName})",
            EvapotranspirationInches = evapotranspirationInches,
            RainfallInches = rainfallInches,
            PlantFactor = plant.WaterFactor,
            GrossNeedInches = grossNeed,
            NetNeedInches = netNeed,
            Explanation = explanation
        };
    }
}
