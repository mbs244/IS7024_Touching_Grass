namespace Touching_Grass.UnitTests;

public sealed class HomepageIntegrationTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public HomepageIntegrationTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task RootPageDisplaysPlantWiseHomepageBrandingAndHeroContent()
    {
        var response = await _host.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("PlantWise", content);
        Assert.Contains("Water smarter. Grow better.", content);
        Assert.Contains("informed watering decisions", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("location", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("plant", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RootPageKeepsWateringWorkflowResponsibilitiesOnExistingPages()
    {
        var response = await _host.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("<form method=\"get\" action=\"/Index\"", content);
        Assert.DoesNotContain("name=\"PlantName\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WaterRecommendation", content, StringComparison.Ordinal);
        Assert.DoesNotContain("ZipLookupService", content, StringComparison.Ordinal);
        Assert.DoesNotContain("WateringCalculatorService", content, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", content, StringComparison.Ordinal);
        Assert.DoesNotContain("fetch(", content, StringComparison.OrdinalIgnoreCase);
    }
}
