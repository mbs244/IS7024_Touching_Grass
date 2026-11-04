using Touching_Grass.Models;

namespace Touching_Grass.Services;

public interface IWateringCalculatorService
{
    Task<WaterRecommendation> CalculateAsync(
        ZipLocation location,
        Plant plant,
        CancellationToken cancellationToken = default);
}
