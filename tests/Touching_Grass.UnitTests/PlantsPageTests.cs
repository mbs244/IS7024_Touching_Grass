using Microsoft.Extensions.Logging.Abstractions;
using Touching_Grass.Models;
using Touching_Grass.Pages;
using Touching_Grass.Repositories;

namespace Touching_Grass.UnitTests;

public sealed class PlantsPageTests
{
    [Fact]
    public async Task NullSearchTermDoesNotQueryRepositoryOrShowNoMatchMessage()
    {
        var repository = new StubPlantRepository([]);
        var page = new PlantsModel(repository, NullLogger<PlantsModel>.Instance)
        {
            SearchTerm = null!
        };

        await page.OnGetAsync(CancellationToken.None);

        Assert.False(page.HasSearched);
        Assert.Empty(page.Results);
        Assert.Null(page.NotFoundMessage);
        Assert.Equal(0, repository.SearchCallCount);
    }

    [Fact]
    public async Task EmptySearchDoesNotQueryRepositoryOrShowNoMatchMessage()
    {
        var repository = new StubPlantRepository([]);
        var page = new PlantsModel(repository, NullLogger<PlantsModel>.Instance)
        {
            SearchTerm = "   "
        };

        await page.OnGetAsync(CancellationToken.None);

        Assert.False(page.HasSearched);
        Assert.Empty(page.Results);
        Assert.Null(page.NotFoundMessage);
        Assert.Equal(0, repository.SearchCallCount);
    }

    [Fact]
    public async Task NonEmptySearchWithNoMatchesShowsExactNotFoundMessage()
    {
        var repository = new StubPlantRepository([]);
        var page = new PlantsModel(repository, NullLogger<PlantsModel>.Instance)
        {
            SearchTerm = "sklujapouetllkjsda"
        };

        await page.OnGetAsync(CancellationToken.None);

        Assert.True(page.HasSearched);
        Assert.Empty(page.Results);
        Assert.Equal("This plant is not in our supported plant database.", page.NotFoundMessage);
        Assert.Null(page.ErrorMessage);
        Assert.Equal("sklujapouetllkjsda", repository.LastSearchTerm);
    }

    [Fact]
    public async Task CatalogLoadFailureShowsUnavailableDataErrorInsteadOfNotFoundMessage()
    {
        var repository = new StubPlantRepository(new InvalidOperationException("Catalog unavailable."));
        var page = new PlantsModel(repository, NullLogger<PlantsModel>.Instance)
        {
            SearchTerm = "salvia"
        };

        await page.OnGetAsync(CancellationToken.None);

        Assert.True(page.HasSearched);
        Assert.Empty(page.Results);
        Assert.Null(page.NotFoundMessage);
        Assert.Equal("Plant data is temporarily unavailable. Please try again.", page.ErrorMessage);
    }

    private sealed class StubPlantRepository : IPlantRepository
    {
        private readonly IReadOnlyList<Plant> _results;
        private readonly Exception? _exception;

        public StubPlantRepository(IReadOnlyList<Plant> results)
        {
            _results = results;
        }

        public StubPlantRepository(Exception exception)
        {
            _results = [];
            _exception = exception;
        }

        public int SearchCallCount { get; private set; }

        public string? LastSearchTerm { get; private set; }

        public Task<PlantMatch> FindAsync(string plantName, CancellationToken cancellationToken = default) =>
            Task.FromResult(PlantMatch.Missing());

        public Task<IReadOnlyList<Plant>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            SearchCallCount++;
            LastSearchTerm = searchTerm;
            return _exception is null
                ? Task.FromResult(_results)
                : Task.FromException<IReadOnlyList<Plant>>(_exception);
        }
    }
}
