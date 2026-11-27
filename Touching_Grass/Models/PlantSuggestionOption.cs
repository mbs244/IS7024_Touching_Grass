namespace Touching_Grass.Models;

public sealed class PlantSuggestionOption
{
    public int PlantId { get; init; }

    public string CommonName { get; init; } = string.Empty;

    public string ScientificName { get; init; } = string.Empty;

    public string DisplayLabel => $"{CommonName} ({ScientificName})";
}
