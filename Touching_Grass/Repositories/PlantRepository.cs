using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.Repositories;

public sealed class PlantRepository : IPlantRepository
{
    private readonly JsonDataLoader _dataLoader;

    public PlantRepository(JsonDataLoader dataLoader)
    {
        _dataLoader = dataLoader;
    }

    public async Task<PlantMatch> FindAsync(string plantName, CancellationToken cancellationToken = default)
    {
        var normalizedName = plantName.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return PlantMatch.Missing();
        }

        var plants = await _dataLoader.LoadAsync<Plant>("plants.json", cancellationToken);
        var matches = plants
            .Where(plant => string.Equals(plant.CommonName, normalizedName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(plant.ScientificName, normalizedName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            0 => PlantMatch.Missing(),
            1 => PlantMatch.Found(matches[0]),
            _ => PlantMatch.Ambiguous()
        };
    }

    public async Task<IReadOnlyList<Plant>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = searchTerm.Trim();
        if (normalizedTerm.Length == 0)
        {
            return [];
        }

        var plants = await _dataLoader.LoadAsync<Plant>("plants.json", cancellationToken);
        return plants
            .Where(plant => plant.CommonName.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase) ||
                            plant.ScientificName.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .ToList();
    }
}
