using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Touching_Grass.Models;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class MyPlantsPageTests
{
    [Fact]
    public async Task RemoveHandler_RemovesOwnedPlantAndShowsNamedConfirmation()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var email = $"remove-{Guid.NewGuid():N}@example.test";
        await RegisterAsync(client, email);

        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);
        var service = scope.ServiceProvider.GetRequiredService<IUserPlantService>();
        var saved = await service.UpsertAfterRecommendationAsync(
            user.Id,
            new Plant { Id = 7, CommonName = "Toyon", ScientificName = "Heteromeles arbutifolia" });

        var listPage = await client.GetAsync("/MyPlants");
        Assert.Equal(HttpStatusCode.OK, listPage.StatusCode);
        var token = ExtractToken(await listPage.Content.ReadAsStringAsync());
        using var removeResponse = await client.PostAsync(
            "/MyPlants?handler=Remove",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["id"] = saved.Id.ToString()
            }));

        Assert.Equal(HttpStatusCode.Redirect, removeResponse.StatusCode);
        Assert.Equal("/MyPlants", removeResponse.Headers.Location?.OriginalString);

        var updatedList = await client.GetAsync("/MyPlants");
        var updatedHtml = await updatedList.Content.ReadAsStringAsync();
        Assert.Contains("Toyon was removed from your plants.", updatedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("Heteromeles arbutifolia", updatedHtml, StringComparison.Ordinal);
        Assert.Empty(await service.ListAsync(user.Id));
    }

    [Fact]
    public async Task RemoveButtonNamesPlantAndDoesNotRemoveAnotherUsersEntry()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var email = $"remove-scope-{Guid.NewGuid():N}@example.test";
        await RegisterAsync(client, email);

        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);
        var service = scope.ServiceProvider.GetRequiredService<LocalUserPlantService>();
        await service.UpsertAfterRecommendationAsync(
            user.Id,
            new Plant { Id = 17, CommonName = "Toyon", ScientificName = "Heteromeles arbutifolia" });
        var otherUserPlant = await service.UpsertAfterRecommendationAsync(
            "other-user",
            new Plant { Id = 17, CommonName = "Toyon", ScientificName = "Heteromeles arbutifolia" });

        var listPage = await client.GetAsync("/MyPlants");
        var listHtml = await listPage.Content.ReadAsStringAsync();
        Assert.Contains("aria-label=\"Remove Toyon from your plants\"", listHtml, StringComparison.Ordinal);
        Assert.Contains(">Remove Toyon</button>", listHtml, StringComparison.Ordinal);

        using var removeResponse = await PostRemovalAsync(client, otherUserPlant.Id);
        Assert.Equal(HttpStatusCode.Redirect, removeResponse.StatusCode);

        var failurePage = await client.GetAsync("/MyPlants");
        var failureHtml = await failurePage.Content.ReadAsStringAsync();
        Assert.Contains("That plant could not be removed from your plants.", failureHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("Toyon was removed from your plants.", failureHtml, StringComparison.Ordinal);
        Assert.NotNull(await service.FindAsync("other-user", otherUserPlant.Id));
        Assert.Single(await service.ListAsync(user.Id));

        using var staleResponse = await PostRemovalAsync(client, int.MaxValue);
        Assert.Equal(HttpStatusCode.Redirect, staleResponse.StatusCode);
        using var stalePage = await client.GetAsync("/MyPlants");
        var staleHtml = await stalePage.Content.ReadAsStringAsync();
        Assert.Contains("That plant could not be removed from your plants.", staleHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("Toyon was removed from your plants.", staleHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemoveHandler_StorageErrorReturnsToMyPlantsWithoutSuccessAndKeepsEntry()
    {
        using var factory = CreateFactory(throwOnRemove: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var email = $"remove-error-{Guid.NewGuid():N}@example.test";
        await RegisterAsync(client, email);

        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);
        var localService = scope.ServiceProvider.GetRequiredService<LocalUserPlantService>();
        var saved = await localService.UpsertAfterRecommendationAsync(
            user.Id,
            new Plant { Id = 23, CommonName = "Toyon", ScientificName = "Heteromeles arbutifolia" });

        using var removeResponse = await PostRemovalAsync(client, saved.Id);
        Assert.Equal(HttpStatusCode.Redirect, removeResponse.StatusCode);
        Assert.Equal("/MyPlants", removeResponse.Headers.Location?.OriginalString);

        var failurePage = await client.GetAsync("/MyPlants");
        var failureHtml = await failurePage.Content.ReadAsStringAsync();
        Assert.Contains("Something went wrong. Your plant was not removed.", failureHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("Toyon was removed from your plants.", failureHtml, StringComparison.Ordinal);
        Assert.NotNull(await localService.FindAsync(user.Id, saved.Id));
    }

    private static WebApplicationFactory<Program> CreateFactory(bool throwOnRemove = false)
    {
        Environment.SetEnvironmentVariable("MYSQLCONNSTR_localdb", null);
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Development");
            builder.UseSetting("applicationName", typeof(Program).Assembly.GetName().Name);
            if (throwOnRemove)
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IUserPlantService>();
                    services.AddSingleton<IUserPlantService>(provider =>
                        new FailingRemoveUserPlantService(
                            provider.GetRequiredService<LocalUserPlantService>()));
                });
            }
        });
    }

    private static async Task<HttpResponseMessage> PostRemovalAsync(HttpClient client, int id)
    {
        using var listPage = await client.GetAsync("/MyPlants");
        var token = ExtractToken(await listPage.Content.ReadAsStringAsync());
        return await client.PostAsync(
            "/MyPlants?handler=Remove",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["id"] = id.ToString()
            }));
    }

    private static async Task RegisterAsync(HttpClient client, string email)
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
                ["DefaultZipCode"] = "45220"
            }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static string ExtractToken(string html)
    {
        const string marker = "name=\"__RequestVerificationToken\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, "Anti-forgery token was not found.");
        const string valueMarker = "value=\"";
        var valueStart = html.IndexOf(valueMarker, start, StringComparison.Ordinal) + valueMarker.Length;
        var valueEnd = html.IndexOf('"', valueStart);
        Assert.True(valueEnd >= valueStart, "Anti-forgery token value was not found.");
        return html[valueStart..valueEnd];
    }

    private sealed class FailingRemoveUserPlantService : IUserPlantService
    {
        private readonly IUserPlantService _inner;

        public FailingRemoveUserPlantService(IUserPlantService inner)
        {
            _inner = inner;
        }

        public Task<IReadOnlyList<UserPlant>> ListAsync(string userId, CancellationToken cancellationToken = default) =>
            _inner.ListAsync(userId, cancellationToken);

        public Task<UserPlant?> FindAsync(string userId, int id, CancellationToken cancellationToken = default) =>
            _inner.FindAsync(userId, id, cancellationToken);

        public Task<UserPlant> UpsertAfterRecommendationAsync(
            string userId,
            Plant plant,
            CancellationToken cancellationToken = default) =>
            _inner.UpsertAfterRecommendationAsync(userId, plant, cancellationToken);

        public Task<bool> ToggleFavoriteAsync(string userId, int id, CancellationToken cancellationToken = default) =>
            _inner.ToggleFavoriteAsync(userId, id, cancellationToken);

        public Task<UserPlant?> RemoveAsync(string userId, int id, CancellationToken cancellationToken = default) =>
            throw new DbUpdateException("Simulated storage failure.");
    }
}
