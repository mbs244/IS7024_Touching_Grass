using System.Text.Json;
using Touching_Grass.Models;

namespace Touching_Grass.Services;

public sealed class JsonDataLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _dataDirectory;

    public JsonDataLoader(IWebHostEnvironment environment)
    {
        _dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
    }

    public async Task<IReadOnlyList<T>> LoadAsync<T>(string fileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_dataDirectory, fileName);

        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"Required local data file '{fileName}' was not found.");
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var records = await JsonSerializer.DeserializeAsync<List<T>>(stream, SerializerOptions, cancellationToken);

            if (records is null)
            {
                throw new InvalidOperationException($"Required local data file '{fileName}' is empty.");
            }

            Validate(records, fileName);
            return records;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Required local data file '{fileName}' is malformed.", exception);
        }
    }

    private static void Validate<T>(IReadOnlyList<T> records, string fileName)
    {
        if (typeof(T) == typeof(Plant))
        {
            var plants = records.Cast<Plant>().ToList();
            if (plants.Any(plant => plant.Id <= 0 || string.IsNullOrWhiteSpace(plant.CommonName) ||
                                   string.IsNullOrWhiteSpace(plant.ScientificName) ||
                                   plant.WaterFactor is < 0 or > 1))
            {
                throw new InvalidOperationException($"Required local data file '{fileName}' contains invalid plant data.");
            }

            if (plants.GroupBy(plant => plant.Id).Any(group => group.Count() > 1))
            {
                throw new InvalidOperationException($"Required local data file '{fileName}' contains duplicate plant identifiers.");
            }
        }
        else if (typeof(T) == typeof(ZipLocation))
        {
            var locations = records.Cast<ZipLocation>().ToList();
            if (locations.Any(location => location.ZipCode.Length != 5 ||
                                          location.ZipCode.Any(character => !char.IsDigit(character)) ||
                                          string.IsNullOrWhiteSpace(location.TimeZoneId) ||
                                          location.Latitude is < -90 or > 90 ||
                                          location.Longitude is < -180 or > 180))
            {
                throw new InvalidOperationException($"Required local data file '{fileName}' contains invalid ZIP location data.");
            }

            if (locations.GroupBy(location => location.ZipCode).Any(group => group.Count() > 1))
            {
                throw new InvalidOperationException($"Required local data file '{fileName}' contains duplicate ZIP codes.");
            }
        }
    }
}
