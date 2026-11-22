using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Touching_Grass.Models;

namespace Touching_Grass.Pages.Account;

public sealed class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    public LogoutModel(SignInManager<ApplicationUser> signInManager) => _signInManager = signInManager;
    public async Task<IActionResult> OnPostAsync() { await _signInManager.SignOutAsync(); return RedirectToPage("/Index"); }
}