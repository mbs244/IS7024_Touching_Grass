using Touching_Grass.Models;

namespace Touching_Grass.Services;

public sealed class ZipLookupService : IZipLookupService
{
    private readonly JsonDataLoader _dataLoader;

    public ZipLookupService(JsonDataLoader dataLoader)
    {
        _dataLoader = dataLoader;
    }

    public async Task<ZipLocation?> FindAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        var normalizedZipCode = zipCode.Trim();
        if (normalizedZipCode.Length != 5 || normalizedZipCode.Any(character => !char.IsDigit(character)))
        {
            return null;
        }

        var locations = await _dataLoader.LoadAsync<ZipLocation>("zip-locations.json", cancellationToken);
        return locations.SingleOrDefault(location => location.ZipCode == normalizedZipCode);
    }
}
