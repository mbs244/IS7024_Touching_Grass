namespace Touching_Grass.Models;

public sealed class Plant
{
    public int Id { get; init; }

    public string CommonName { get; init; } = string.Empty;

    public string ScientificName { get; init; } = string.Empty;

    public decimal WaterFactor { get; init; }
}
