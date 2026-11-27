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
        Assert.Contains("Dynamic watering advice for your plants that knows the weather.", content);
        Assert.Contains("id=\"watering-form\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"ZipCode\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"PlantName\"", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RootPageOwnsUnifiedWateringWorkflowResponsibilities()
    {
        var response = await _host.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("<form method=\"post\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"SelectedPlantId\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"plant-suggestions\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("id=\"recommendation-heading\"", content, StringComparison.OrdinalIgnoreCase);
    }
}
