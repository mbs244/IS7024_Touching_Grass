using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using System.Net;
using Touching_Grass.Data;
using Touching_Grass.Models;
using Touching_Grass.Repositories;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class RazorPagesTestHost : IAsyncLifetime
{
    private WebApplication? _application;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var contentRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Touching_Grass"));
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(Touching_Grass.Pages.IndexModel).Assembly.GetName().Name,
            ContentRootPath = contentRootPath,
            EnvironmentName = "Development"
        });
        builder.Configuration["VisualCrossing:ApiKey"] = "test-host-key";

        builder.WebHost.UseTestServer();
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("Touching_Grass.TestHost.Identity"));
        builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<LocalUserPlantService>();
        builder.Services.AddSingleton<IUserPlantService>(services =>
            services.GetRequiredService<LocalUserPlantService>());
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AddPageRoute("/Index", "/Index");
        });
        builder.Services.AddSingleton<JsonDataLoader>();
        builder.Services.AddSingleton<IZipLookupService, ZipLookupService>();
        builder.Services.AddSingleton<IPlantRepository, PlantRepository>();
        builder.Services.AddHttpClient<OpenMeteoService>(client =>
                client.BaseAddress = new Uri("https://api.open-meteo.com/"))
            .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler(request =>
            {
                var timeZoneParameter = request.RequestUri!.Query
                    .Split('&')
                    .Single(parameter => parameter.StartsWith("timezone=", StringComparison.Ordinal));
                var timeZone = Uri.UnescapeDataString(timeZoneParameter["timezone=".Length..]);
                var period = WeatherPeriod.PreviousSevenCompleteDays(new ZipLocation
                {
                    TimeZoneId = timeZone
                });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(WeatherApiTestHelpers.OpenMeteoResponse(period, 1m))
                };
            }));
        builder.Services.AddHttpClient<VisualCrossingService>(client =>
                client.BaseAddress = new Uri("https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/"))
            .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler(request =>
            {
                var pathSegments = request.RequestUri!.AbsolutePath.Split('/');
                var period = new WeatherPeriod(
                    DateOnly.ParseExact(pathSegments[^2], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    DateOnly.ParseExact(pathSegments[^1], "yyyy-MM-dd", CultureInfo.InvariantCulture));
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(WeatherApiTestHelpers.VisualCrossingResponse(period, 0.1m))
                };
            }));
        builder.Services.AddScoped<IWateringCalculatorService, WateringCalculatorService>();
        builder.Services.AddSingleton<IWaterStatusMessageService, WaterStatusMessageService>();

        _application = builder.Build();
        _application.UseRouting();
        _application.UseAuthentication();
        _application.UseAuthorization();
        _application.MapRazorPages();

        await _application.StartAsync();
        Client = _application.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (_application is not null)
        {
            await _application.StopAsync();
            await _application.DisposeAsync();
        }
    }
}
