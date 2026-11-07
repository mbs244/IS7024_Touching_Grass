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
        Assert.Contains("<section", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<h1 id=\"homepage-title\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<h2 id=\"get-started-heading\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<h2 id=\"how-it-works-heading\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("aria-labelledby=\"homepage-title\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("aria-labelledby=\"get-started-heading\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("aria-labelledby=\"how-it-works-heading\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("src=\"/images/plantwise-hero.png\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alt=\"Lush indoor plants gathered in a bright, welcoming space\"", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HomepageZipInputHasAssociatedLabelAndAccessibleValidationRequirements()
    {
        var content = await GetHomepageAsync();

        Assert.Contains("<label for=\"zipCode\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"zipCode\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"zipCode\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("required", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("pattern=\"[0-9]{5}\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("minlength=\"5\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("maxlength=\"5\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("aria-describedby=\"zip-help zip-format-help\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"zip-help\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"zip-format-help\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("five digits", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HomepageNavigationAndGetStartedControlHaveMeaningfulNativeMarkup()
    {
        var content = await GetHomepageAsync();

        var navigation = Regex.Match(content, "<nav\\b[\\s\\S]*?</nav>", RegexOptions.IgnoreCase);
        Assert.True(navigation.Success, "The homepage layout navigation was not found.");

        var navigationContent = navigation.Value;
        Assert.Contains(">Home</a>", navigationContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(">Watering Guide</a>", navigationContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(">Privacy</a>", navigationContent, StringComparison.OrdinalIgnoreCase);

        var homeIndex = navigationContent.IndexOf(">Home</a>", StringComparison.OrdinalIgnoreCase);
        var wateringIndex = navigationContent.IndexOf(">Watering Guide</a>", StringComparison.OrdinalIgnoreCase);
        var privacyIndex = navigationContent.IndexOf(">Privacy</a>", StringComparison.OrdinalIgnoreCase);
        Assert.True(homeIndex >= 0 && homeIndex < wateringIndex && wateringIndex < privacyIndex,
            "Navigation links must appear in Home, Watering Guide, Privacy order.");

        var startForm = Regex.Match(content, "<form\\b[^>]*action=\"/Index\"[\\s\\S]*?</form>", RegexOptions.IgnoreCase);
        Assert.True(startForm.Success, "The Get Started form targeting /Index was not found.");
        Assert.Contains("method=\"get\"", startForm.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<button type=\"submit\"", startForm.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(">Get Started</button>", startForm.Value, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> GetHomepageAsync()
    {
        var response = await _host.Client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
