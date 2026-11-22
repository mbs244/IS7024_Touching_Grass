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

    public async Task<Plant?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var plants = await _dataLoader.LoadAsync<Plant>("plants.json", cancellationToken);
        return plants.SingleOrDefault(plant => plant.Id == id);
    }
}
