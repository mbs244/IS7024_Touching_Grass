using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Touching_Grass.Models;
using Touching_Grass.Pages;
using Touching_Grass.Repositories;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class PlantAutocompleteHandlerTests
{
    [Fact]
    public async Task QueryShorterThanThreeCharacters_ReturnsNoSuggestions()
    {
        var model = CreateModel(new StubPlantRepository([]));

        var action = await model.OnGetAutocompleteAsync("ma", CancellationToken.None);

        var payload = GetPayload(action);
        Assert.Equal(0, GetProperty(payload.RootElement, "suggestions").GetArrayLength());
    }

    [Fact]
    public async Task QueryUsesCaseInsensitiveContainsAndCapsAtTenSuggestions()
    {
        var plants = Enumerable.Range(1, 12)
            .Select(index => new Plant
            {
                Id = index,
                CommonName = $"Maple {index}",
                ScientificName = $"Acer example {index}",
                WaterFactor = 0.5m
            })
            .ToList();

        var model = CreateModel(new StubPlantRepository(plants));

        var action = await model.OnGetAutocompleteAsync("MAP", CancellationToken.None);

        var payload = GetPayload(action);
        var suggestions = GetProperty(payload.RootElement, "suggestions");
        Assert.Equal(10, suggestions.GetArrayLength());
        Assert.Contains("Maple 1", GetProperty(suggestions[0], "commonName").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task QueryWithNoMatches_ReturnsFriendlyNoResultsMessage()
    {
        var model = CreateModel(new StubPlantRepository([]));

        var action = await model.OnGetAutocompleteAsync("zzz", CancellationToken.None);

        var payload = GetPayload(action);
        Assert.Equal(0, GetProperty(payload.RootElement, "suggestions").GetArrayLength());
        Assert.Equal(
            "No matching plants found. Try another common or scientific name.",
            GetProperty(payload.RootElement, "noResultsMessage").GetString());
    }

    private static IndexModel CreateModel(IPlantRepository repository)
    {
        var model = new IndexModel(
            repository,
            new StubWateringCalculatorService(),
            new StubZipLookupService(),
            NullLogger<IndexModel>.Instance,
            new StubUserPlantService(),
            new TestUserManager(),
            new WaterStatusMessageService())
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        return model;
    }

    private static JsonDocument GetPayload(IActionResult action)
    {
        var result = Assert.IsType<JsonResult>(action);
        return JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
    }

    private static JsonElement GetProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var lower))
        {
            return lower;
        }

        var pascal = char.ToUpperInvariant(propertyName[0]) + propertyName[1..];
        if (element.TryGetProperty(pascal, out var upper))
        {
            return upper;
        }

        throw new KeyNotFoundException(propertyName);
    }

    private sealed class StubPlantRepository : IPlantRepository
    {
        private readonly IReadOnlyList<Plant> _plants;

        public StubPlantRepository(IReadOnlyList<Plant> plants)
        {
            _plants = plants;
        }

        public Task<PlantMatch> FindAsync(string plantName, CancellationToken cancellationToken = default) => Task.FromResult(PlantMatch.Missing());

        public Task<IReadOnlyList<Plant>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var matches = _plants
                .Where(plant => plant.CommonName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                                || plant.ScientificName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult<IReadOnlyList<Plant>>(matches);
        }

        public Task<Plant?> FindByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Plant?>(null);
    }

    private sealed class StubZipLookupService : IZipLookupService
    {
        public Task<ZipLocation?> FindAsync(string zipCode, CancellationToken cancellationToken = default) => Task.FromResult<ZipLocation?>(null);
    }

    private sealed class StubWateringCalculatorService : IWateringCalculatorService
    {
        public Task<WaterRecommendation> CalculateAsync(ZipLocation location, Plant plant, CancellationToken cancellationToken = default) => Task.FromResult(new WaterRecommendation());
    }

    private sealed class StubUserPlantService : IUserPlantService
    {
        public Task<IReadOnlyList<UserPlant>> ListAsync(string userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<UserPlant>>([]);
        public Task<UserPlant?> FindAsync(string userId, int id, CancellationToken cancellationToken = default) => Task.FromResult<UserPlant?>(null);
        public Task<UserPlant> UpsertAfterRecommendationAsync(string userId, Plant plant, CancellationToken cancellationToken = default) => Task.FromResult(new UserPlant());
        public Task<bool> ToggleFavoriteAsync(string userId, int id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<UserPlant?> RemoveAsync(string userId, int id, CancellationToken cancellationToken = default) => Task.FromResult<UserPlant?>(null);
    }

    private sealed class TestUserManager : UserManager<ApplicationUser>
    {
        public TestUserManager()
            : base(
                new EmptyUserStore(),
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
    }

    private sealed class EmptyUserStore : IUserStore<ApplicationUser>
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
}
