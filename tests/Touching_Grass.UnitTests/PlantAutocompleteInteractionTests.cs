namespace Touching_Grass.UnitTests;

public sealed class PlantAutocompleteInteractionTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public PlantAutocompleteInteractionTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task LandingPageContainsAutocompleteInteractionElements()
    {
        var response = await _host.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("data-plant-input", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("data-plant-id", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"plant-suggestions\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"plant-search-status\"", content, StringComparison.OrdinalIgnoreCase);
    }
}
