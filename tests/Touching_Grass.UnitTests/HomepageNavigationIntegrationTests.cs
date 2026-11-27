namespace Touching_Grass.UnitTests;

public sealed class HomepageNavigationIntegrationTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public HomepageNavigationIntegrationTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task HomepageDisplaysHowItWorksStepsInOrder()
    {
        var response = await _host.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("id=\"watering-form\"", content);
        Assert.Contains("name=\"ZipCode\"", content);
        Assert.Contains("name=\"PlantName\"", content);
    }

    [Fact]
    public async Task HomepageNavigationProvidesLabeledLinksToExistingDestinations()
    {
        var response = await _host.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        AssertLink(content, "navbar-brand", "/", "PlantWise");
        AssertLink(content, "nav-link text-dark", "/", "Watering Guide");
        AssertLink(content, "nav-link text-dark", "/Plants", "Plant Search");
        AssertLink(content, "nav-link text-dark", "/Privacy", "Privacy");
        Assert.Contains("navbar navbar-expand-sm navbar-toggleable-sm", content);
        Assert.Contains("navbar-toggler", content);
        Assert.Contains("data-bs-toggle=\"collapse\"", content);
        Assert.Contains("data-bs-target=\".navbar-collapse\"", content);

        foreach (var path in new[] { "/", "/Plants", "/Privacy" })
        {
            var destinationResponse = await _host.Client.GetAsync(path);
            destinationResponse.EnsureSuccessStatusCode();
        }
    }

    private static void AssertLink(string content, string cssClass, string destination, string label)
    {
        var anchorStartIndex = 0;
        while ((anchorStartIndex = content.IndexOf("<a", anchorStartIndex, StringComparison.Ordinal)) >= 0)
        {
            var tagEndIndex = content.IndexOf('>', anchorStartIndex);
            if (tagEndIndex < 0)
            {
                break;
            }

            var openingTag = content[anchorStartIndex..tagEndIndex];
            var closingTagIndex = content.IndexOf("</a>", tagEndIndex, StringComparison.Ordinal);
            if (openingTag.Contains($"class=\"{cssClass}\"", StringComparison.Ordinal)
                && openingTag.Contains($"href=\"{destination}\"", StringComparison.Ordinal)
                && closingTagIndex > tagEndIndex
                && content[tagEndIndex..closingTagIndex].Contains(label, StringComparison.Ordinal))
            {
                return;
            }

            anchorStartIndex = tagEndIndex + 1;
        }

        Assert.Fail($"The {label} link with destination {destination} was not found.");
    }
}
