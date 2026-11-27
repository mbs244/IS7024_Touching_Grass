using Microsoft.EntityFrameworkCore;
using Touching_Grass.Data;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class UserPlantServiceTests
{
    [Fact]
    public async Task RemoveAsync_RemovesAndReturnsOnlyTheOwnedEntry()
    {
        await using var context = CreateContext();
        var owned = new UserPlant
        {
            UserId = "user-a",
            SourcePlantId = 7,
            CommonNameSnapshot = "Toyon",
            ScientificNameSnapshot = "Heteromeles arbutifolia"
        };
        var otherUserPlant = new UserPlant
        {
            UserId = "user-b",
            SourcePlantId = 7,
            CommonNameSnapshot = "Toyon",
            ScientificNameSnapshot = "Heteromeles arbutifolia"
        };
        context.UserPlants.AddRange(owned, otherUserPlant);
        await context.SaveChangesAsync();

        var service = new UserPlantService(context);
        var removed = await service.RemoveAsync("user-a", owned.Id);

        Assert.NotNull(removed);
        Assert.Equal(owned.Id, removed.Id);
        Assert.Empty(await service.ListAsync("user-a"));
        Assert.Equal(otherUserPlant.Id, Assert.Single(await service.ListAsync("user-b")).Id);
    }

    [Fact]
    public async Task RemoveAsync_ReturnsNullForMissingOrNonOwnedEntry()
    {
        await using var context = CreateContext();
        var owned = new UserPlant
        {
            UserId = "user-a",
            SourcePlantId = 8,
            CommonNameSnapshot = "Basil",
            ScientificNameSnapshot = "Ocimum basilicum"
        };
        context.UserPlants.Add(owned);
        await context.SaveChangesAsync();

        var service = new UserPlantService(context);

        Assert.Null(await service.RemoveAsync("user-b", owned.Id));
        Assert.Null(await service.RemoveAsync("user-a", owned.Id + 100));
        Assert.Equal(owned.Id, Assert.Single(await service.ListAsync("user-a")).Id);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
