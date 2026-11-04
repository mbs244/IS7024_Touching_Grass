using Touching_Grass.Repositories;
using Touching_Grass.Services;

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

    private sealed class LocalDataFixture : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "PlantWiseTests", Guid.NewGuid().ToString("N"));

        public LocalDataFixture()
        {
            var dataDirectory = Path.Combine(_root, "Data");
            Directory.CreateDirectory(dataDirectory);
            File.WriteAllText(Path.Combine(dataDirectory, "zip-locations.json"), """
                [{"ZipCode":"95814","Latitude":38.5816,"Longitude":-121.4944,"TimeZoneId":"America/Los_Angeles"}]
                """);
            File.WriteAllText(Path.Combine(dataDirectory, "plants.json"), """
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
