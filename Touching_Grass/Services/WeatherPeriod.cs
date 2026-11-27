using Touching_Grass.Models;

namespace Touching_Grass.Services;

public readonly record struct WeatherPeriod(DateOnly StartDate, DateOnly EndDate)
{
    public const int DayCount = 7;

    public static WeatherPeriod PreviousSevenCompleteDays(ZipLocation location)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId);
            var localToday = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone).DateTime);
            var endDate = localToday.AddDays(-1);

            return new WeatherPeriod(endDate.AddDays(-(DayCount - 1)), endDate);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new WeatherDataUnavailableException();
        }
        catch (InvalidTimeZoneException)
        {
            throw new WeatherDataUnavailableException();
        }
        catch (ArgumentException)
        {
            throw new WeatherDataUnavailableException();
        }
    }
}
