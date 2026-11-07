using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.Pages.Account;

[Authorize]
public sealed class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IZipLookupService _zipLookupService;
    public ProfileModel(UserManager<ApplicationUser> userManager, IZipLookupService zipLookupService) { _userManager = userManager; _zipLookupService = zipLookupService; }
    [BindProperty, Required, RegularExpression(@"^\d{5}$")] public string DefaultZipCode { get; set; } = string.Empty;
    public async Task OnGetAsync() => DefaultZipCode = (await _userManager.GetUserAsync(User))?.DefaultZipCode ?? string.Empty;
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        DefaultZipCode = DefaultZipCode.Trim();
        if (ModelState.IsValid && await _zipLookupService.FindAsync(DefaultZipCode, cancellationToken) is null) ModelState.AddModelError(nameof(DefaultZipCode), "That ZIP code is not supported.");
        if (!ModelState.IsValid) { DefaultZipCode = user.DefaultZipCode; return Page(); }
        user.DefaultZipCode = DefaultZipCode;
        await _userManager.UpdateAsync(user);
        ViewData["Success"] = "Your default ZIP code was updated.";
        return Page();
    }
}