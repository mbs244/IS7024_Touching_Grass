using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Touching_Grass.Models;
using Touching_Grass.Repositories;

namespace Touching_Grass.Pages;

public sealed class PlantsModel : PageModel
{
    private readonly IPlantRepository _plantRepository;
    private readonly ILogger<PlantsModel> _logger;

    public PlantsModel(IPlantRepository plantRepository, ILogger<PlantsModel> logger)
    {
        _plantRepository = plantRepository;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = string.Empty;

    public IReadOnlyList<Plant> Results { get; private set; } = [];

    public bool HasSearched { get; private set; }

    public string? NotFoundMessage { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        SearchTerm = SearchTerm?.Trim() ?? string.Empty;
        if (SearchTerm.Length == 0)
        {
            return;
        }

        HasSearched = true;
        try
        {
            Results = await _plantRepository.SearchAsync(SearchTerm, cancellationToken);
            if (Results.Count == 0)
            {
                NotFoundMessage = "This plant is not in our supported plant database.";
            }
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Local PlantWise plant data could not be loaded.");
            ErrorMessage = "Plant data is temporarily unavailable. Please try again.";
        }
    }
}
