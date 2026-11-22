using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.Pages.MyPlants;

[Authorize]
public sealed class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserPlantService _service;
    public IndexModel(UserManager<ApplicationUser> userManager, IUserPlantService service) { _userManager = userManager; _service = service; }
    public IReadOnlyList<UserPlant> Plants { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken cancellationToken) => Plants = await _service.ListAsync(_userManager.GetUserId(User)!, cancellationToken);
    public async Task<IActionResult> OnPostToggleFavoriteAsync(int id, CancellationToken cancellationToken)
    { await _service.ToggleFavoriteAsync(_userManager.GetUserId(User)!, id, cancellationToken); return RedirectToPage(); }
}