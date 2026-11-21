using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.Pages;

public sealed class ResultsModel : PageModel
{
    private readonly ILogger<ResultsModel> _logger;
    private readonly IWaterStatusMessageService _waterStatusMessageService;

    public ResultsModel(ILogger<ResultsModel> logger, IWaterStatusMessageService waterStatusMessageService)
    {
        _logger = logger;
        _waterStatusMessageService = waterStatusMessageService;
    }

    [BindProperty(SupportsGet = true)]
    public string ZipCode { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string PlantName { get; set; } = string.Empty;

    public WaterRecommendation? Recommendation { get; private set; }

    public WaterStatusMessage? WaterStatusMessage { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
        var serializedRecommendation = TempData["WaterRecommendation"] as string;
        if (string.IsNullOrWhiteSpace(serializedRecommendation))
        {
            ErrorMessage = "The recommendation is no longer available. Start a new lookup.";
            return;
        }

        try
        {
            var recommendation = JsonSerializer.Deserialize<WaterRecommendation>(serializedRecommendation);
            if (recommendation is null)
            {
                ErrorMessage = "The recommendation could not be displayed. Start a new lookup.";
                return;
            }

            WaterStatusMessage = _waterStatusMessageService.Create(recommendation.NetNeedInches);
            Recommendation = recommendation;
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "The PlantWise recommendation could not be displayed.");
            ErrorMessage = "The recommendation could not be displayed. Start a new lookup.";
        }
        catch (ArgumentOutOfRangeException exception)
        {
            _logger.LogError(exception, "The PlantWise recommendation contains an invalid net watering need.");
            ErrorMessage = "The recommendation could not be displayed. Start a new lookup.";
        }
    }
}
