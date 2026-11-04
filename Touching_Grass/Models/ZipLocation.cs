namespace Touching_Grass.Models;

public sealed class ZipLocation
{
    public string ZipCode { get; init; } = string.Empty;

    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }

    public string TimeZoneId { get; init; } = string.Empty;
}
