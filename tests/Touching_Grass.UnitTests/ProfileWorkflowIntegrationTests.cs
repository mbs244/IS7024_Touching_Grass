using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Touching_Grass.UnitTests;

public sealed class ProfileWorkflowIntegrationTests
{
    [Fact]
    public async Task UpdatingProfileZip_PersistsAndPrefillsUnifiedLandingWorkflow()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var email = $"profile-workflow-{Guid.NewGuid():N}@example.test";
        await RegisterAsync(client, email, "45220");

        var profilePage = await client.GetAsync("/Account/Profile");
        var profileHtml = await profilePage.Content.ReadAsStringAsync();
        profilePage.EnsureSuccessStatusCode();

        var token = ExtractToken(profileHtml);
        using var saveResponse = await client.PostAsync(
            "/Account/Profile",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["DefaultZipCode"] = "10001"
            }));

        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var landingResponse = await client.GetAsync("/");
        var landingHtml = await landingResponse.Content.ReadAsStringAsync();
        landingResponse.EnsureSuccessStatusCode();

        Assert.Contains("My Profile", landingHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("value=\"10001\"", landingHtml, StringComparison.OrdinalIgnoreCase);
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
