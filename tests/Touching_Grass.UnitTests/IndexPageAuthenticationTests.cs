using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Touching_Grass.Models;
using Touching_Grass.Pages;
using Touching_Grass.Repositories;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class IndexPageAuthenticationTests
{
    [Fact]
    public async Task AnonymousUser_SeesEmptyZipFieldOnLandingPage()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var zipInput = Regex.Match(
            content,
            "<input[^>]*name=\"ZipCode\"[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.True(zipInput.Success, "ZIP input was not rendered.");
        Assert.DoesNotContain("value=\"452", zipInput.Value, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuthenticatedUser_WithPreferredZip_SeesPrefilledZipField()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var email = $"zip-prefill-{Guid.NewGuid():N}@example.test";
        await RegisterAsync(client, email, "45220");

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var zipInput = Regex.Match(
            content,
            "<input[^>]*name=\"ZipCode\"[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.True(zipInput.Success, "ZIP input was not rendered.");
        Assert.Contains("value=\"45220\"", zipInput.Value, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OnGet_Unauthenticated_DoesNotCallUserManagerLookup()
    {
        var userManager = new RecordingUserManager();
        var page = CreatePageModel(userManager, new ClaimsPrincipal(new ClaimsIdentity()));

        await page.OnGetAsync();

        Assert.Equal(0, userManager.GetUserAsyncCalls);
        Assert.Equal(string.Empty, page.ZipCode);
    }

    [Fact]
    public async Task OnGet_Authenticated_CallsUserManagerLookupAndPrefillsZip()
    {
        var userManager = new RecordingUserManager
        {
            UserToReturn = new ApplicationUser { DefaultZipCode = "10001" }
        };

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], authenticationType: "Test");
        var page = CreatePageModel(userManager, new ClaimsPrincipal(identity));

        await page.OnGetAsync();

        Assert.Equal(1, userManager.GetUserAsyncCalls);
        Assert.Equal("10001", page.ZipCode);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Development");
            builder.UseSetting("applicationName", typeof(Program).Assembly.GetName().Name);
        });
    }

    private static IndexModel CreatePageModel(UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
    {
        var model = new IndexModel(
            new StubPlantRepository(),
            new StubWateringCalculatorService(),
            new StubZipLookupService(),
            NullLogger<IndexModel>.Instance,
            new StubUserPlantService(),
            userManager,
            new WaterStatusMessageService())
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };

        return model;
    }

    private static async Task RegisterAsync(HttpClient client, string email, string zip)
    {
        var registerPage = await client.GetAsync("/Account/Register");
        var token = ExtractToken(await registerPage.Content.ReadAsStringAsync());

        using var response = await client.PostAsync(
            "/Account/Register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Email"] = email,
                ["Password"] = "LocalTest1",
                ["ConfirmPassword"] = "LocalTest1",
                ["DefaultZipCode"] = zip
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static string ExtractToken(string html)
    {
        var tokenMatch = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.True(tokenMatch.Success, "The anti-forgery token was not found.");
        return tokenMatch.Groups[1].Value;
    }

    private sealed class RecordingUserManager : UserManager<ApplicationUser>
    {
        public RecordingUserManager()
            : base(
                new InMemoryUserStore(),
                Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new ServiceCollection().BuildServiceProvider(),
                NullLogger<UserManager<ApplicationUser>>.Instance)
        {
        }

        public int GetUserAsyncCalls { get; private set; }

        public ApplicationUser? UserToReturn { get; init; }

        public override Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
        {
            GetUserAsyncCalls++;
            return Task.FromResult(UserToReturn);
        }
    }

    private sealed class InMemoryUserStore : IUserStore<ApplicationUser>
    {
        public void Dispose() { }
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
    }

    private sealed class StubPlantRepository : IPlantRepository
    {
        public Task<PlantMatch> FindAsync(string plantName, CancellationToken cancellationToken = default) => Task.FromResult(PlantMatch.Missing());
        public Task<IReadOnlyList<Plant>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Plant>>([]);
        public Task<Plant?> FindByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Plant?>(null);
    }

    private sealed class StubWateringCalculatorService : IWateringCalculatorService
    {
        public Task<WaterRecommendation> CalculateAsync(ZipLocation location, Plant plant, CancellationToken cancellationToken = default) =>
            Task.FromResult(new WaterRecommendation());
    }

    private sealed class StubZipLookupService : IZipLookupService
    {
        public Task<ZipLocation?> FindAsync(string zipCode, CancellationToken cancellationToken = default) => Task.FromResult<ZipLocation?>(null);
    }

    private sealed class StubUserPlantService : IUserPlantService
    {
        public Task<IReadOnlyList<UserPlant>> ListAsync(string userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<UserPlant>>([]);
        public Task<UserPlant?> FindAsync(string userId, int id, CancellationToken cancellationToken = default) => Task.FromResult<UserPlant?>(null);
        public Task<UserPlant> UpsertAfterRecommendationAsync(string userId, Plant plant, CancellationToken cancellationToken = default) => Task.FromResult(new UserPlant());
        public Task<bool> ToggleFavoriteAsync(string userId, int id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<UserPlant?> RemoveAsync(string userId, int id, CancellationToken cancellationToken = default) => Task.FromResult<UserPlant?>(null);
    }
}
