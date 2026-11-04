using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Touching_Grass.Models;

namespace Touching_Grass.Pages;

public sealed class ResultsModel : PageModel
{
    private readonly ILogger<ResultsModel> _logger;

    public ResultsModel(ILogger<ResultsModel> logger)
    {
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string ZipCode { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string PlantName { get; set; } = string.Empty;

    public WaterRecommendation? Recommendation { get; private set; }

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
            Recommendation = JsonSerializer.Deserialize<WaterRecommendation>(serializedRecommendation);
            if (Recommendation is null)
            {
                ErrorMessage = "The recommendation could not be displayed. Start a new lookup.";
            }
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "The PlantWise recommendation could not be displayed.");
            ErrorMessage = "The recommendation could not be displayed. Start a new lookup.";
        }
    }
}
