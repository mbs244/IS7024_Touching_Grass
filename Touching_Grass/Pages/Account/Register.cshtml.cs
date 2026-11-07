using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.Pages.Account;

public sealed class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IZipLookupService _zipLookupService;

    public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IZipLookupService zipLookupService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _zipLookupService = zipLookupService;
    }

    [BindProperty]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required, RegularExpression(@"^\d{5}$")]
    public string DefaultZipCode { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Email = Email.Trim();
        DefaultZipCode = DefaultZipCode.Trim();
        if (ModelState.IsValid && await _zipLookupService.FindAsync(DefaultZipCode, cancellationToken) is null)
        {
            ModelState.AddModelError(nameof(DefaultZipCode), "That ZIP code is not supported.");
        }

        if (!ModelState.IsValid)
        {
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            return Page();
        }

        var user = new ApplicationUser { UserName = Email, Email = Email, DefaultZipCode = DefaultZipCode };
        var result = await _userManager.CreateAsync(user, Password);
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Code == "DuplicateUserName" ? "That email address is already registered." : error.Description);
            }
            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToPage("/Index");
    }
}