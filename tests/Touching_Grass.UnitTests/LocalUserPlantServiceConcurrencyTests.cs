using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class LocalUserPlantServiceConcurrencyTests
{
    [Fact]
    public async Task ConcurrentUpsertsForSameUserAndSourcePlant_DoNotCreateDuplicates()
    {
        var service = new LocalUserPlantService();
        const string userId = "user-concurrency";
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var operations = Enumerable.Range(0, 48)
            .Select(index => Task.Run(async () =>
            {
                await start.Task;
                return await service.UpsertAfterRecommendationAsync(
                    userId,
                    new Plant
                    {
                        Id = 9,
                        CommonName = $"Rose-{index}",
                        ScientificName = $"Rosa-{index}"
                    });
            }))
            .ToArray();

        start.SetResult();
        var results = await Task.WhenAll(operations);

        Assert.Single(results.Select(result => result.Id).Distinct());

        var allPlants = await service.ListAsync(userId);
        var saved = Assert.Single(allPlants);
        Assert.Equal(9, saved.SourcePlantId);
    }

    [Fact]
    public async Task ConcurrentTogglesFollowParityAndDoNotCorruptState()
    {
        var service = new LocalUserPlantService();
        const string userId = "user-toggle";
        var saved = await service.UpsertAfterRecommendationAsync(
            userId,
            new Plant { Id = 14, CommonName = "Basil", ScientificName = "Ocimum basilicum" });

        const int toggleCount = 101;
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var toggles = Enumerable.Range(0, toggleCount)
            .Select(_ => Task.Run(async () =>
            {
                await start.Task;
                return await service.ToggleFavoriteAsync(userId, saved.Id);
            }))
            .ToArray();

        start.SetResult();
        var toggleResults = await Task.WhenAll(toggles);

        Assert.All(toggleResults, Assert.True);

        var updated = await service.FindAsync(userId, saved.Id);
        Assert.NotNull(updated);
        Assert.True(updated.IsFavorite);
    }

    [Fact]
    public async Task ConcurrentReadsDuringMutationReturnConsistentClonedSnapshots()
    {
        var service = new LocalUserPlantService();
        const string userId = "user-read";
        var saved = await service.UpsertAfterRecommendationAsync(
            userId,
            new Plant { Id = 21, CommonName = "Pair-A", ScientificName = "Sci-A" });

        var variants = new[]
        {
            new Plant { Id = 21, CommonName = "Pair-A", ScientificName = "Sci-A" },
            new Plant { Id = 21, CommonName = "Pair-B", ScientificName = "Sci-B" }
        };

        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(700));

        var writer = Task.Run(async () =>
        {
            var index = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                await service.UpsertAfterRecommendationAsync(userId, variants[index % variants.Length], cts.Token);
                index++;
            }
        });

        var readers = Enumerable.Range(0, 4)
            .Select(_ => Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var list = await service.ListAsync(userId, cts.Token);
                    var listed = Assert.Single(list);
                    Assert.True(IsValidPair(listed.CommonNameSnapshot, listed.ScientificNameSnapshot));

                    var found = await service.FindAsync(userId, saved.Id, cts.Token);
                    Assert.NotNull(found);
                    Assert.True(IsValidPair(found.CommonNameSnapshot, found.ScientificNameSnapshot));
                }
            }))
            .ToArray();

        await Task.Delay(700);
        cts.Cancel();

        await AwaitWithOptionalCancellationAsync(writer);
        await Task.WhenAll(readers.Select(AwaitWithOptionalCancellationAsync));

        var externalSnapshot = await service.FindAsync(userId, saved.Id);
        Assert.NotNull(externalSnapshot);
        externalSnapshot.CommonNameSnapshot = "external-mutation";

        var stored = await service.FindAsync(userId, saved.Id);
        Assert.NotNull(stored);
        Assert.NotEqual("external-mutation", stored.CommonNameSnapshot);
    }

    private static bool IsValidPair(string commonName, string scientificName) =>
        (commonName, scientificName) is ("Pair-A", "Sci-A") or ("Pair-B", "Sci-B");

    private static async Task AwaitWithOptionalCancellationAsync(Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is observed mid-loop.
        }
    }
}
