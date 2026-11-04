namespace Touching_Grass.Models;

public sealed class WaterRecommendation
{
    public string ZipCode { get; init; } = string.Empty;

    public string PlantDisplayName { get; init; } = string.Empty;

    public decimal EvapotranspirationInches { get; init; }

    public decimal RainfallInches { get; init; }

    public decimal PlantFactor { get; init; }

    public decimal GrossNeedInches { get; init; }

    public decimal NetNeedInches { get; init; }

    public string Explanation { get; init; } = string.Empty;
}
