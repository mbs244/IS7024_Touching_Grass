namespace Touching_Grass.Services;

public enum WeatherMetric
{
    Evapotranspiration,
    Rainfall
}

public sealed record WeatherObservation(
    WeatherMetric Metric,
    decimal TotalInches,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string TimeZoneId,
    string Source);
