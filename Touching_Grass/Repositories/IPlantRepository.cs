using Touching_Grass.Models;

namespace Touching_Grass.Repositories;

public enum PlantMatchStatus
{
    Found,
    Missing,
    Ambiguous
}

public sealed record PlantMatch(PlantMatchStatus Status, Plant? Plant)
{
    public static PlantMatch Found(Plant plant) => new(PlantMatchStatus.Found, plant);

    public static PlantMatch Missing() => new(PlantMatchStatus.Missing, null);

    public static PlantMatch Ambiguous() => new(PlantMatchStatus.Ambiguous, null);
}

public interface IPlantRepository
{
    Task<PlantMatch> FindAsync(string plantName, CancellationToken cancellationToken = default);

    Task<Plant?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
}
