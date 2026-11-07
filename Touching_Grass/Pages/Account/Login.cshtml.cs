using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Touching_Grass.Models;

namespace Touching_Grass.Pages.Account;

public sealed class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    public LoginModel(SignInManager<ApplicationUser> signInManager) => _signInManager = signInManager;
    [BindProperty, Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [BindProperty, Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        Email = Email.Trim();
        if (!ModelState.IsValid) { Password = string.Empty; return Page(); }
        var result = await _signInManager.PasswordSignInAsync(Email, Password, false, false);
        Password = string.Empty;
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, "Invalid email or password."); return Page(); }
        return LocalRedirect(returnUrl ?? Url.Content("~/"));
    }
}