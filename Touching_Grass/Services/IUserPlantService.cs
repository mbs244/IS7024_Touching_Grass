using Touching_Grass.Models;

namespace Touching_Grass.Services;

public interface IUserPlantService
{
    Task<IReadOnlyList<UserPlant>> ListAsync(string userId, CancellationToken cancellationToken = default);

    Task<UserPlant?> FindAsync(string userId, int id, CancellationToken cancellationToken = default);

    Task<UserPlant> UpsertAfterRecommendationAsync(
        string userId,
        Plant plant,
        CancellationToken cancellationToken = default);

    Task<bool> ToggleFavoriteAsync(string userId, int id, CancellationToken cancellationToken = default);
}