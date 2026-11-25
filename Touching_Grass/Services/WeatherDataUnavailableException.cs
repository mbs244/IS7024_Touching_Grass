namespace Touching_Grass.Services;

public sealed class WeatherDataUnavailableException : Exception
{
    public const string UserMessage = "Weather data is temporarily unavailable. Please try again.";

    public WeatherDataUnavailableException()
        : base(UserMessage)
    {
    }
}
