namespace Touching_Grass.UnitTests;

public sealed class NavigationAndTitleConsistencyTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public NavigationAndTitleConsistencyTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Theory]
    [InlineData("/", "Plantwise - Watering Guide")]
    [InlineData("/Plants", "Plantwise - Plant Search")]
    [InlineData("/Privacy", "Plantwise - Privacy")]
    [InlineData("/Account/Login", "Plantwise - Sign In")]
    [InlineData("/Account/Register", "Plantwise - Create Account")]
    public async Task PageTitles_ArePlantwiseAndDoNotExposeInternalProjectName(string path, string expectedTitle)
    {
        var response = await _host.Client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        var titleMatch = System.Text.RegularExpressions.Regex.Match(
            content,
            "<title>(.*?)</title>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

        Assert.True(titleMatch.Success, "Page title element was not found.");
        Assert.Equal(expectedTitle, titleMatch.Groups[1].Value.Trim());
        Assert.DoesNotContain("Touching_Grass", titleMatch.Groups[1].Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Navigation_UsesWateringGuideAndPlantSearchLabels()
    {
        var response = await _host.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains(">Watering Guide</a>", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(">Plant Search</a>", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(">Home</a>", content, StringComparison.OrdinalIgnoreCase);
    }
}
