using System.Globalization;
using Touching_Grass.Models;

namespace Touching_Grass.Services;

public sealed class WaterStatusMessageService : IWaterStatusMessageService
{
    private const decimal StandardWateringThresholdInches = 0.25m;

    public WaterStatusMessage Create(decimal netNeedInches)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(netNeedInches);

        var roundedNeedInches = decimal.Round(netNeedInches, 2, MidpointRounding.AwayFromZero);
        var message = roundedNeedInches switch
        {
            0m => "No extra watering needed this week.",
            < StandardWateringThresholdInches => $"Water lightly this week: about {roundedNeedInches.ToString("0.00", CultureInfo.InvariantCulture)} in.",
            _ => $"Water about {roundedNeedInches.ToString("0.00", CultureInfo.InvariantCulture)} in this week."
        };

        return new WaterStatusMessage { Message = message };
    }
}
