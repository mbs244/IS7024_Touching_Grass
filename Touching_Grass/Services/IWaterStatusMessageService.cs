using Touching_Grass.Models;

namespace Touching_Grass.Services;

public interface IWaterStatusMessageService
{
    WaterStatusMessage Create(decimal netNeedInches);
}
