using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;

namespace Touching_Grass.UnitTests;

internal sealed class TestHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "Touching_Grass.UnitTests";

    public string EnvironmentName { get; set; } = "Development";

    public string WebRootPath { get; set; } = string.Empty;

    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

    public string ContentRootPath { get; set; } = string.Empty;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
