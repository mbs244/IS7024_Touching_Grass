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
        return _db.UserPlants
            .AsNoTracking()
            .SingleOrDefaultAsync(plant => plant.UserId == userId && plant.Id == id, cancellationToken);
    }

    public async Task<UserPlant> UpsertAfterRecommendationAsync(
        string userId,
        Plant plant,
        CancellationToken cancellationToken = default)
    {
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
        var savedPlant = await _db.UserPlants
            .SingleOrDefaultAsync(plant => plant.UserId == userId && plant.Id == id, cancellationToken);
        if (savedPlant is null)
        {
            return false;
        }

        savedPlant.IsFavorite = !savedPlant.IsFavorite;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}