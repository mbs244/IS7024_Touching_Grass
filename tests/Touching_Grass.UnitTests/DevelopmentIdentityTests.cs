using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Touching_Grass.Models;
using Touching_Grass.Pages.Account;

namespace Touching_Grass.UnitTests;

public sealed class DevelopmentIdentityTests
{
    [Fact]
    public async Task DevelopmentHost_ResolvesIdentityAndAccountPagesWithoutConnectionString()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>());
        Assert.NotNull(ActivatorUtilities.CreateInstance<LoginModel>(scope.ServiceProvider));
        Assert.NotNull(ActivatorUtilities.CreateInstance<RegisterModel>(scope.ServiceProvider));

        var response = await client.GetAsync("/Account/Register");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DevelopmentHost_CanRegisterSignInAndSignOutInOneProcess()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var registerPage = await client.GetAsync("/Account/Register");
        var registerHtml = await registerPage.Content.ReadAsStringAsync();
        var token = ExtractToken(registerHtml);
        var email = $"local-{Guid.NewGuid():N}@example.test";
        using var register = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = email,
            ["Password"] = "LocalTest1",
            ["ConfirmPassword"] = "LocalTest1",
            ["DefaultZipCode"] = "45220"
        }));

        Assert.Equal(HttpStatusCode.Redirect, register.StatusCode);
        Assert.Contains(register.Headers.GetValues("Set-Cookie"), cookie => cookie.Contains(".AspNetCore.Identity.Application"));

        var logoutPage = await client.GetAsync("/Account/Logout");
        var logoutToken = ExtractToken(await logoutPage.Content.ReadAsStringAsync());
        using var logout = await client.PostAsync("/Account/Logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = logoutToken
        }));
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        var loginPage = await client.GetAsync("/Account/Login");
        var loginToken = ExtractToken(await loginPage.Content.ReadAsStringAsync());
        using var login = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = loginToken,
            ["Email"] = email,
            ["Password"] = "LocalTest1"
        }));

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
    }

    [Fact]
    public void DevelopmentHost_UsesInMemoryProviderAndDoesNotUseMySql()
    {
        using var factory = CreateFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();

        Assert.Contains("InMemory", context.Database.ProviderName, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MySql", context.Database.ProviderName, StringComparison.OrdinalIgnoreCase);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        Environment.SetEnvironmentVariable("MYSQLCONNSTR_localdb", null);
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Development");
            builder.UseSetting("applicationName", typeof(Program).Assembly.GetName().Name);
        });
    }

    private static string ExtractToken(string html)
    {
        var marker = "name=\"__RequestVerificationToken\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, "Anti-forgery token was not found.");
        var valueMarker = "value=\"";
        var valueStart = html.IndexOf(valueMarker, start, StringComparison.Ordinal) + valueMarker.Length;
        var valueEnd = html.IndexOf('"', valueStart);
        return html[valueStart..valueEnd];
    }
}
