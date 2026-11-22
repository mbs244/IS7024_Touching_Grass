using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class LocalUserPlantServiceTests
{
    [Fact]
    public async Task UpsertListAndToggleAreUserScopedAndPersistAcrossRequests()
    {
        var service = new LocalUserPlantService();
        var plant = new Plant { Id = 7, CommonName = "Rose", ScientificName = "Rosa" };

        var saved = await service.UpsertAfterRecommendationAsync("user-a", plant);
        Assert.Single(await service.ListAsync("user-a"));
        Assert.Empty(await service.ListAsync("user-b"));

        Assert.True(await service.ToggleFavoriteAsync("user-a", saved.Id));
        var favorite = await service.FindAsync("user-a", saved.Id);
        Assert.NotNull(favorite);
        Assert.True(favorite.IsFavorite);
        Assert.Null(await service.FindAsync("user-b", saved.Id));
    }

    [Fact]
    public async Task UpsertSamePlantUpdatesExistingRecordForOneUser()
    {
        var service = new LocalUserPlantService();
        var first = await service.UpsertAfterRecommendationAsync(
            "user-a",
            new Plant { Id = 7, CommonName = "Rose", ScientificName = "Rosa" });

        var second = await service.UpsertAfterRecommendationAsync(
            "user-a",
            new Plant { Id = 7, CommonName = "Garden rose", ScientificName = "Rosa rubiginosa" });

        Assert.Equal(first.Id, second.Id);
        var plants = await service.ListAsync("user-a");
        var saved = Assert.Single(plants);
        Assert.Equal("Garden rose", saved.CommonNameSnapshot);
    }
}
