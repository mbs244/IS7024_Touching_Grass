using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Touching_Grass.Models;
using Touching_Grass.Repositories;
using Touching_Grass.Services;

namespace Touching_Grass.Pages;

public class IndexModel : PageModel
{
    private const int MinimumAutocompleteLength = 3;
    private const int MaximumAutocompleteSuggestions = 10;

    private readonly IPlantRepository _plantRepository;
    private readonly IWateringCalculatorService _wateringCalculator;
    private readonly IZipLookupService _zipLookupService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IUserPlantService _userPlantService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWaterStatusMessageService _waterStatusMessageService;

    public IndexModel(
        IPlantRepository plantRepository,
        IWateringCalculatorService wateringCalculator,
        IZipLookupService zipLookupService,
        ILogger<IndexModel> logger,
        IUserPlantService userPlantService,
        UserManager<ApplicationUser> userManager,
        IWaterStatusMessageService waterStatusMessageService)
    {
        _plantRepository = plantRepository;
        _wateringCalculator = wateringCalculator;
        _zipLookupService = zipLookupService;
        _logger = logger;
        _userPlantService = userPlantService;
        _userManager = userManager;
        _waterStatusMessageService = waterStatusMessageService;
    }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "ZIP code")]
    [Required(ErrorMessage = "ZIP code is required.")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "Enter a valid five-digit ZIP code.")]
    public string ZipCode { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Plant name")]
    [Required(ErrorMessage = "Plant name is required.")]
    public string PlantName { get; set; } = string.Empty;

    [BindProperty]
    public int? SelectedPlantId { get; set; }

    public WaterRecommendation? Recommendation { get; private set; }

    public WaterStatusMessage? WaterStatusMessage { get; private set; }

    public string? RecommendationError { get; private set; }

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (!string.IsNullOrWhiteSpace(currentUser?.DefaultZipCode))
        {
            ZipCode = currentUser.DefaultZipCode;
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ZipCode = ZipCode.Trim();
        PlantName = PlantName.Trim();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var location = await _zipLookupService.FindAsync(ZipCode, cancellationToken);
            if (location is null)
            {
                ModelState.AddModelError(nameof(ZipCode), "That ZIP code is not supported.");
                return Page();
            }

            Plant? plant = null;
            if (SelectedPlantId is int selectedPlantId)
            {
                plant = await _plantRepository.FindByIdAsync(selectedPlantId, cancellationToken);
            }

            if (plant is null)
            {
                var plantMatch = await _plantRepository.FindAsync(PlantName, cancellationToken);
                if (plantMatch.Status == PlantMatchStatus.Missing)
                {
                    ModelState.AddModelError(nameof(PlantName), "That plant is not in the supported plant dataset.");
                    return Page();
                }

                if (plantMatch.Status == PlantMatchStatus.Ambiguous || plantMatch.Plant is null)
                {
                    ModelState.AddModelError(nameof(PlantName), "Enter a more specific plant name or choose a suggestion.");
                    return Page();
                }

                plant = plantMatch.Plant;
            }

            var recommendation = await _wateringCalculator.CalculateAsync(location, plant, cancellationToken);
            if (User.FindFirstValue(ClaimTypes.NameIdentifier) is string userId)
            {
                try
                {
                    await _userPlantService.UpsertAfterRecommendationAsync(userId, plant, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception,
                        "Optional user-plant recommendation save failed for user {UserId}; returning the calculation result.",
                        userId);
                }
            }

            Recommendation = recommendation;
            WaterStatusMessage = _waterStatusMessageService.Create(recommendation.NetNeedInches);
            RecommendationError = null;
            return Page();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ModelState.AddModelError(string.Empty, "The request was canceled. Please try again.");
            return Page();
        }
        catch (WeatherDataUnavailableException)
        {
            RecommendationError = WeatherDataUnavailableException.UserMessage;
            ModelState.AddModelError(string.Empty, WeatherDataUnavailableException.UserMessage);
            return Page();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Local PlantWise data could not be loaded.");
            RecommendationError = "PlantWise data is temporarily unavailable. Please try again.";
            ModelState.AddModelError(string.Empty, "PlantWise data is temporarily unavailable. Please try again.");
            return Page();
        }
    }

    public async Task<IActionResult> OnGetAutocompleteAsync(string query, CancellationToken cancellationToken)
    {
        var normalizedQuery = query?.Trim() ?? string.Empty;
        if (normalizedQuery.Length < MinimumAutocompleteLength)
        {
            return new JsonResult(new { suggestions = Array.Empty<PlantSuggestionOption>(), noResultsMessage = (string?)null });
        }

        try
        {
            var matches = await _plantRepository.SearchAsync(normalizedQuery, cancellationToken);
            var suggestions = matches
                .Take(MaximumAutocompleteSuggestions)
                .Select(plant => new PlantSuggestionOption
                {
                    PlantId = plant.Id,
                    CommonName = plant.CommonName,
                    ScientificName = plant.ScientificName
                })
                .ToList();

            var noResultsMessage = suggestions.Count == 0
                ? "No matching plants found. Try another common or scientific name."
                : null;

            return new JsonResult(new { suggestions, noResultsMessage });
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Plant autocomplete lookup failed.");
            return new JsonResult(new
            {
                suggestions = Array.Empty<PlantSuggestionOption>(),
                noResultsMessage = "Plant suggestions are temporarily unavailable."
            });
        }
    }
}
