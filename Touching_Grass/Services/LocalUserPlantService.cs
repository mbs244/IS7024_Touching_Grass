using Touching_Grass.Models;

namespace Touching_Grass.Services;

/// <summary>
/// Keeps Development mode database-free while allowing pages that display optional
/// user-owned data to be constructed and exercised locally.
/// </summary>
public sealed class LocalUserPlantService : IUserPlantService
{
    public Task<IReadOnlyList<UserPlant>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return Task.FromResult<IReadOnlyList<UserPlant>>([]);
    }

    public Task<UserPlant?> FindAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return Task.FromResult<UserPlant?>(null);
    }

    public Task<UserPlant> UpsertAfterRecommendationAsync(
        string userId,
        Plant plant,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "Persistent user-plant data is available only in Azure persistence mode.");
    }

    public Task<bool> ToggleFavoriteAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return Task.FromResult(false);
    }
}
