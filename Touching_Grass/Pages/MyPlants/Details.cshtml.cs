using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Touching_Grass.Models;
using Touching_Grass.Repositories;
using Touching_Grass.Services;

namespace Touching_Grass.Pages.MyPlants;

[Authorize]
public sealed class DetailsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users; private readonly IUserPlantService _plants; private readonly IPlantRepository _repository; private readonly IZipLookupService _zips; private readonly IWateringCalculatorService _calculator;
    public DetailsModel(UserManager<ApplicationUser> users, IUserPlantService plants, IPlantRepository repository, IZipLookupService zips, IWateringCalculatorService calculator) { _users = users; _plants = plants; _repository = repository; _zips = zips; _calculator = calculator; }
    public UserPlant? SavedPlant { get; private set; } public WaterRecommendation? Recommendation { get; private set; } public string? ErrorMessage { get; private set; }
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    { SavedPlant = await _plants.FindAsync(_users.GetUserId(User)!, id, cancellationToken); if (SavedPlant is null) return NotFound(); var plant = await _repository.FindByIdAsync(SavedPlant.SourcePlantId, cancellationToken); if (plant is null) { ErrorMessage = "This plant is no longer available for recommendations."; return Page(); } var user = await _users.GetUserAsync(User); var location = await _zips.FindAsync(user!.DefaultZipCode, cancellationToken); if (location is null) { ErrorMessage = "Your saved ZIP code is unavailable. Update it from your profile."; return Page(); } Recommendation = await _calculator.CalculateAsync(location, plant, cancellationToken); return Page(); }
}