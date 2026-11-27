using Microsoft.EntityFrameworkCore;
using Touching_Grass.Data;
using Touching_Grass.Models;

namespace Touching_Grass.Services;

public sealed class UserPlantService : IUserPlantService
{
    private readonly ApplicationDbContext _db;

    public UserPlantService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UserPlant>> ListAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return await _db.UserPlants
            .AsNoTracking()
            .Where(plant => plant.UserId == userId)
            .OrderByDescending(plant => plant.IsFavorite)
            .ThenBy(plant => plant.CommonNameSnapshot)
            .ThenBy(plant => plant.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<UserPlant?> FindAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return _db.UserPlants
            .AsNoTracking()
            .SingleOrDefaultAsync(plant => plant.UserId == userId && plant.Id == id, cancellationToken);
    }

    public async Task<UserPlant> UpsertAfterRecommendationAsync(
        string userId,
        Plant plant,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var now = DateTime.UtcNow;
        var savedPlant = await _db.UserPlants
            .SingleOrDefaultAsync(
                entry => entry.UserId == userId && entry.SourcePlantId == plant.Id,
                cancellationToken);

        if (savedPlant is null)
        {
            savedPlant = new UserPlant
            {
                UserId = userId,
                SourcePlantId = plant.Id,
                CommonNameSnapshot = plant.CommonName,
                ScientificNameSnapshot = plant.ScientificName,
                CreatedAt = now,
                LastSearchedAt = now
            };
            _db.UserPlants.Add(savedPlant);
        }
        else
        {
            savedPlant.CommonNameSnapshot = plant.CommonName;
            savedPlant.ScientificNameSnapshot = plant.ScientificName;
            savedPlant.LastSearchedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return savedPlant;
    }

    public async Task<bool> ToggleFavoriteAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var updated = await _db.UserPlants
            .Where(plant => plant.UserId == userId && plant.Id == id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(plant => plant.IsFavorite, plant => !plant.IsFavorite),
                cancellationToken);
        return updated > 0;
    }

    public async Task<UserPlant?> RemoveAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var savedPlant = await _db.UserPlants
            .SingleOrDefaultAsync(
                plant => plant.UserId == userId && plant.Id == id,
                cancellationToken);
        if (savedPlant is null)
        {
            return null;
        }

        _db.UserPlants.Remove(savedPlant);
        await _db.SaveChangesAsync(cancellationToken);
        return savedPlant;
    }
}