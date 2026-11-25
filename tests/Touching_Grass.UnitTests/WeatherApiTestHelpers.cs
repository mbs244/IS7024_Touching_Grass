using System.Text.Json;
using System.Globalization;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

internal static class WeatherApiTestHelpers
{
    public static ZipLocation Location { get; } = new()
    {
        ZipCode = "95814",
        Latitude = 38.5816m,
        Longitude = -121.4944m,
        TimeZoneId = "America/Los_Angeles"
    };

    public static WeatherPeriod Period => WeatherPeriod.PreviousSevenCompleteDays(Location);

    public static string OpenMeteoResponse(WeatherPeriod period, decimal dailyValue)
    {
        var dates = Enumerable.Range(0, WeatherPeriod.DayCount)
            .Select(day => period.StartDate.AddDays(day).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Append(period.EndDate.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .ToArray();
        var values = Enumerable.Repeat<decimal?>(dailyValue, WeatherPeriod.DayCount)
            .Append(null)
            .ToArray();

        return JsonSerializer.Serialize(new
        {
            daily = new
            {
                time = dates,
                et0_fao_evapotranspiration = values
            }
        });
    }

    public static string VisualCrossingResponse(WeatherPeriod period, decimal dailyValue) =>
        JsonSerializer.Serialize(new
        {
            days = Enumerable.Range(0, WeatherPeriod.DayCount)
                .Select(day => new
                {
                    datetime = period.StartDate.AddDays(day).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    precip = dailyValue
                })
        });
}
