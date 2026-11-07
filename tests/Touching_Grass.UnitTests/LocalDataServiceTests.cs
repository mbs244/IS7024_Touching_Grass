using Touching_Grass.Repositories;
using Touching_Grass.Services;
using System.Text.Json;

namespace Touching_Grass.UnitTests;

public sealed class LocalDataServiceTests
{
    [Fact]
    public async Task ZipLookupTrimsInputAndFindsSupportedLocation()
    {
        using var fixture = new LocalDataFixture();
        var service = new ZipLookupService(fixture.Loader);

        var result = await service.FindAsync(" 95814 ");

        Assert.NotNull(result);
        Assert.Equal("America/Los_Angeles", result.TimeZoneId);
    }

    [Fact]
    public async Task ZipLookupFindsSupportedNonCaliforniaLocation()
    {
        using var fixture = new LocalDataFixture();
        var service = new ZipLookupService(fixture.Loader);

        var result = await service.FindAsync("45236");

        Assert.NotNull(result);
        Assert.Equal("America/New_York", result.TimeZoneId);
    }

    [Fact]
    public async Task ZipLookupReturnsNullForMalformedOrUnsupportedZip()
    {
        using var fixture = new LocalDataFixture();
        var service = new ZipLookupService(fixture.Loader);

        Assert.Null(await service.FindAsync("9581"));
        Assert.Null(await service.FindAsync("99999"));
    }

    [Fact]
    public async Task PlantRepositoryMatchesCommonAndScientificNamesCaseInsensitively()
    {
        using var fixture = new LocalDataFixture();
        var repository = new PlantRepository(fixture.Loader);

        var commonNameResult = await repository.FindAsync(" toyon ");
        var scientificNameResult = await repository.FindAsync("HETEROMELES ARBUTIFOLIA");

        Assert.Equal(PlantMatchStatus.Found, commonNameResult.Status);
        Assert.Equal(PlantMatchStatus.Found, scientificNameResult.Status);
        Assert.Equal(0.40m, commonNameResult.Plant!.WaterFactor);
    }

    [Fact]
    public async Task PlantRepositoryReturnsMissingForUnknownPlant()
    {
        using var fixture = new LocalDataFixture();
        var repository = new PlantRepository(fixture.Loader);

        var result = await repository.FindAsync("Unknown plant");

        Assert.Equal(PlantMatchStatus.Missing, result.Status);
        Assert.Null(result.Plant);
    }

    [Fact]
    public async Task PlantRepositorySearchMatchesPartialCommonOrScientificNamesIgnoringCaseAndWhitespace()
    {
        using var fixture = new LocalDataFixture("""
            [
              {"Id":1,"CommonName":"white sage","ScientificName":"Salvia apiana","WaterFactor":0.2},
              {"Id":2,"CommonName":"toyon","ScientificName":"Heteromeles arbutifolia","WaterFactor":0.4},
              {"Id":3,"CommonName":"lavender","ScientificName":"Lavandula angustifolia","WaterFactor":0.5}
            ]
            """);
        var repository = new PlantRepository(fixture.Loader);

        var salviaResults = await repository.SearchAsync(" SALVIA ");
        var toyonResults = await repository.SearchAsync("TOYON");
        var scientificNameResults = await repository.SearchAsync("ARBUTIFOLIA");

        Assert.Equal("white sage", Assert.Single(salviaResults).CommonName);
        Assert.Equal("toyon", Assert.Single(toyonResults).CommonName);
        Assert.Equal("toyon", Assert.Single(scientificNameResults).CommonName);
    }

    [Fact]
    public async Task PlantRepositorySearchReturnsAtMostFiftyMatchesInCatalogOrder()
    {
        var plants = Enumerable.Range(1, 55)
            .Select(id => new
            {
                Id = id,
                CommonName = $"matching plant {id}",
                ScientificName = $"Botanicalus plantus {id}",
                WaterFactor = 0.4m
            });
        using var fixture = new LocalDataFixture(JsonSerializer.Serialize(plants));
        var repository = new PlantRepository(fixture.Loader);

        var results = await repository.SearchAsync("MATCHING");

        Assert.Equal(50, results.Count);
        Assert.Equal("matching plant 1", results[0].CommonName);
        Assert.Equal("matching plant 50", results[^1].CommonName);
    }

    [Fact]
    public async Task PlantRepositorySearchReturnsNoResultsForEmptyOrUnknownTerms()
    {
        using var fixture = new LocalDataFixture();
        var repository = new PlantRepository(fixture.Loader);

        Assert.Empty(await repository.SearchAsync("   "));
        Assert.Empty(await repository.SearchAsync("sklujapouetllkjsda"));
    }

    private sealed class LocalDataFixture : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "PlantWiseTests", Guid.NewGuid().ToString("N"));

        public LocalDataFixture(string? plantsJson = null)
        {
            var dataDirectory = Path.Combine(_root, "Data");
            Directory.CreateDirectory(dataDirectory);
            File.WriteAllText(Path.Combine(dataDirectory, "zip-locations.json"), """
                [{"ZipCode":"95814","Latitude":38.5816,"Longitude":-121.4944,"TimeZoneId":"America/Los_Angeles"},{"ZipCode":"45236","Latitude":39.2351,"Longitude":-84.3533,"TimeZoneId":"America/New_York"}]
                """);
            File.WriteAllText(Path.Combine(dataDirectory, "plants.json"), plantsJson ?? """
                [{"Id":1,"CommonName":"Toyon","ScientificName":"Heteromeles arbutifolia","WaterFactor":0.40}]
                """);

            Loader = new JsonDataLoader(new TestHostEnvironment { ContentRootPath = _root });
        }

        public JsonDataLoader Loader { get; }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
    }
}
