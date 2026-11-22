using Touching_Grass.Models;
using System.Collections.Concurrent;
using System.Threading;

namespace Touching_Grass.Services;

/// <summary>
/// Keeps Development mode database-free while allowing pages that display optional
/// user-owned data to be constructed and exercised locally. The collection is
/// intentionally process-local and is not a substitute for Azure persistence.
/// </summary>
public sealed class LocalUserPlantService : IUserPlantService
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, UserPlant>> _plantsByUser = new();
    private int _nextId;

    public Task<IReadOnlyList<UserPlant>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_plantsByUser.TryGetValue(userId, out var plants))
        {
            return Task.FromResult<IReadOnlyList<UserPlant>>([]);
        }

        var result = plants.Values
            .OrderByDescending(plant => plant.IsFavorite)
            .ThenBy(plant => plant.CommonNameSnapshot)
            .ThenBy(plant => plant.Id)
            .Select(Clone)
            .ToArray();
        return Task.FromResult<IReadOnlyList<UserPlant>>(result);
    }

    public Task<UserPlant?> FindAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        cancellationToken.ThrowIfCancellationRequested();

        var found = _plantsByUser.TryGetValue(userId, out var plants) && plants.TryGetValue(id, out var plant)
            ? Clone(plant)
            : null;
        return Task.FromResult(found);
    }

    public Task<UserPlant> UpsertAfterRecommendationAsync(
        string userId,
        Plant plant,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(plant);
        cancellationToken.ThrowIfCancellationRequested();

        var userPlants = _plantsByUser.GetOrAdd(userId, _ => new ConcurrentDictionary<int, UserPlant>());
        var now = DateTime.UtcNow;
        var savedPlant = userPlants.Values.FirstOrDefault(entry => entry.SourcePlantId == plant.Id);
        if (savedPlant is null)
        {
            savedPlant = new UserPlant
            {
                Id = Interlocked.Increment(ref _nextId),
                UserId = userId,
                SourcePlantId = plant.Id,
                CommonNameSnapshot = plant.CommonName,
                ScientificNameSnapshot = plant.ScientificName,
                CreatedAt = now,
                LastSearchedAt = now
            };
            userPlants[savedPlant.Id] = savedPlant;
        }
        else
        {
            savedPlant.CommonNameSnapshot = plant.CommonName;
            savedPlant.ScientificNameSnapshot = plant.ScientificName;
            savedPlant.LastSearchedAt = now;
        }

        return Task.FromResult(Clone(savedPlant));
    }

    public Task<bool> ToggleFavoriteAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_plantsByUser.TryGetValue(userId, out var plants) || !plants.TryGetValue(id, out var plant))
        {
            return Task.FromResult(false);
        }

        plant.IsFavorite = !plant.IsFavorite;
        return Task.FromResult(true);
    }

    private static UserPlant Clone(UserPlant plant) => new()
    {
        Id = plant.Id,
        UserId = plant.UserId,
        SourcePlantId = plant.SourcePlantId,
        CommonNameSnapshot = plant.CommonNameSnapshot,
        ScientificNameSnapshot = plant.ScientificNameSnapshot,
        IsFavorite = plant.IsFavorite,
        CreatedAt = plant.CreatedAt,
        LastSearchedAt = plant.LastSearchedAt
    };
}
