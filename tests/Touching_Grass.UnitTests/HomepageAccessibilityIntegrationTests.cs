using System.Text.RegularExpressions;

namespace Touching_Grass.UnitTests;

public sealed class HomepageAccessibilityIntegrationTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public HomepageAccessibilityIntegrationTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task HomepageProvidesSemanticMainHeadingAndSections()
    {
        var content = await GetHomepageAsync();

        Assert.Contains("<main", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<h1", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"watering-form\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("data-plant-input", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"plant-suggestions\"", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HomepageZipInputHasAssociatedLabelAndAccessibleValidationRequirements()
    {
        var content = await GetHomepageAsync();

        Assert.Contains("name=\"ZipCode\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("autocomplete=\"postal-code\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("inputmode=\"numeric\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("aria-describedby=\"zip-help\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"zip-help\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("five-digit", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HomepageNavigationAndGetStartedControlHaveMeaningfulNativeMarkup()
    {
        var content = await GetHomepageAsync();

        var navigation = Regex.Match(content, "<nav\\b[\\s\\S]*?</nav>", RegexOptions.IgnoreCase);
        Assert.True(navigation.Success, "The homepage layout navigation was not found.");

        var navigationContent = navigation.Value;
        Assert.Contains(">Watering Guide</a>", navigationContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(">Plant Search</a>", navigationContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(">Privacy</a>", navigationContent, StringComparison.OrdinalIgnoreCase);

        var wateringIndex = navigationContent.IndexOf(">Watering Guide</a>", StringComparison.OrdinalIgnoreCase);
        var searchIndex = navigationContent.IndexOf(">Plant Search</a>", StringComparison.OrdinalIgnoreCase);
        var privacyIndex = navigationContent.IndexOf(">Privacy</a>", StringComparison.OrdinalIgnoreCase);
        Assert.True(wateringIndex >= 0 && wateringIndex < searchIndex && searchIndex < privacyIndex,
            "Navigation links must appear in Watering Guide, Plant Search, Privacy order.");

        var startForm = Regex.Match(content, "<form\\b[^>]*id=\"watering-form\"[\\s\\S]*?</form>", RegexOptions.IgnoreCase);
        Assert.True(startForm.Success, "The watering form was not found.");
        Assert.Contains("method=\"post\"", startForm.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<button type=\"submit\"", startForm.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Calculate watering need", startForm.Value, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> GetHomepageAsync()
    {
        var response = await _host.Client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
