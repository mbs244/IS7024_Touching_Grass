using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        builder.WebHost.UseTestServer();
        builder.Services.AddRazorPages();
        builder.Services.AddSingleton<JsonDataLoader>();
        builder.Services.AddSingleton<IZipLookupService, ZipLookupService>();
        builder.Services.AddSingleton<IPlantRepository, PlantRepository>();
        builder.Services.AddSingleton<IWateringCalculatorService, WateringCalculatorService>();

        _application = builder.Build();
        _application.UseRouting();
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
