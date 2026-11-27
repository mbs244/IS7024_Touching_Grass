using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Touching_Grass.UnitTests;

public sealed class ProfilePageRegressionTests
{
    [Fact]
    public async Task ProfilePage_CanBeViewedAfterRegistration()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var email = $"profile-view-{Guid.NewGuid():N}@example.test";
        await RegisterAsync(client, email, "45220");

        var response = await client.GetAsync("/Account/Profile");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("<h1>Profile</h1>", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"DefaultZipCode\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("value=\"45220\"", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProfilePage_AllowsEditingAndSavingPreferredZipCode()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var email = $"profile-save-{Guid.NewGuid():N}@example.test";
        await RegisterAsync(client, email, "45220");

        var profilePage = await client.GetAsync("/Account/Profile");
        var profileHtml = await profilePage.Content.ReadAsStringAsync();
        profilePage.EnsureSuccessStatusCode();

        var token = ExtractToken(profileHtml);
        using var response = await client.PostAsync(
            "/Account/Profile",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["DefaultZipCode"] = "10001"
            }));

        var updatedHtml = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Your default ZIP code was updated.", updatedHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("value=\"10001\"", updatedHtml, StringComparison.OrdinalIgnoreCase);
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
}
