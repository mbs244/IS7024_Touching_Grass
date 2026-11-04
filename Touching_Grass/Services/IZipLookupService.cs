using Touching_Grass.Models;

namespace Touching_Grass.Services;

public interface IZipLookupService
{
    Task<ZipLocation?> FindAsync(string zipCode, CancellationToken cancellationToken = default);
}
