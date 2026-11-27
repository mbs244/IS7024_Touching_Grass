using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.Pages.MyPlants;

[Authorize]
public sealed class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserPlantService _service;
    private readonly ILogger<IndexModel> _logger;
    public IndexModel(UserManager<ApplicationUser> userManager, IUserPlantService service, ILogger<IndexModel> logger)
    {
        _userManager = userManager;
        _service = service;
        _logger = logger;
    }
    public IReadOnlyList<UserPlant> Plants { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken cancellationToken) => Plants = await _service.ListAsync(_userManager.GetUserId(User)!, cancellationToken);
    public async Task<IActionResult> OnPostToggleFavoriteAsync(int id, CancellationToken cancellationToken)
    { await _service.ToggleFavoriteAsync(_userManager.GetUserId(User)!, id, cancellationToken); return RedirectToPage(); }

    public async Task<IActionResult> OnPostRemoveAsync(int id, CancellationToken cancellationToken)
    {
        TempData.Remove("SuccessMessage");
        TempData.Remove("ErrorMessage");

        try
        {
            var removedPlant = await _service.RemoveAsync(_userManager.GetUserId(User)!, id, cancellationToken);
            if (removedPlant is null)
            {
                TempData["ErrorMessage"] = "That plant could not be removed from your plants.";
            }
            else
            {
                TempData["SuccessMessage"] = $"{removedPlant.CommonNameSnapshot} was removed from your plants.";
            }
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Failed to remove a user's plant from persistent storage.");
            TempData["ErrorMessage"] = "Something went wrong. Your plant was not removed.";
        }
        catch (DbException exception)
        {
            _logger.LogError(exception, "Failed to remove a user's plant from persistent storage.");
            TempData["ErrorMessage"] = "Something went wrong. Your plant was not removed.";
        }

        return RedirectToPage();
    }
}