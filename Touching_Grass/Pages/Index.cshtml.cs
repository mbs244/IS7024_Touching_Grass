using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Touching_Grass.Repositories;
using Touching_Grass.Services;

namespace Touching_Grass.Pages;

public class IndexModel : PageModel
{
    private readonly IPlantRepository _plantRepository;
    private readonly IWateringCalculatorService _wateringCalculator;
    private readonly IZipLookupService _zipLookupService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IPlantRepository plantRepository,
        IWateringCalculatorService wateringCalculator,
        IZipLookupService zipLookupService,
        ILogger<IndexModel> logger)
    {
        _plantRepository = plantRepository;
        _wateringCalculator = wateringCalculator;
        _zipLookupService = zipLookupService;
        _logger = logger;
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

            var plantMatch = await _plantRepository.FindAsync(PlantName, cancellationToken);
            if (plantMatch.Status == PlantMatchStatus.Missing)
            {
                ModelState.AddModelError(nameof(PlantName), "That plant is not in the supported plant dataset.");
                return Page();
            }

            if (plantMatch.Status == PlantMatchStatus.Ambiguous || plantMatch.Plant is null)
            {
                ModelState.AddModelError(nameof(PlantName), "Enter a more specific plant name.");
                return Page();
            }

            var recommendation = await _wateringCalculator.CalculateAsync(location, plantMatch.Plant, cancellationToken);
            TempData["WaterRecommendation"] = JsonSerializer.Serialize(recommendation);
            return RedirectToPage("/Results", new { zipCode = ZipCode, plantName = PlantName });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ModelState.AddModelError(string.Empty, "The request was canceled. Please try again.");
            return Page();
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Local PlantWise data could not be loaded.");
            ModelState.AddModelError(string.Empty, "PlantWise data is temporarily unavailable. Please try again.");
            return Page();
        }
    }
}
