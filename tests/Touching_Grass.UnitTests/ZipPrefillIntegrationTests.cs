using System.Text.RegularExpressions;

namespace Touching_Grass.UnitTests;

public sealed class ZipPrefillIntegrationTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public ZipPrefillIntegrationTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task IndexGetWithZipCodePrefillsExistingWateringFormWithoutCalculating()
    {
        var response = await _host.Client.GetAsync("/Index?zipCode=45236");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("id=\"watering-form\"", content);
        Assert.Contains("method=\"post\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"PlantName\"", content);
        Assert.DoesNotContain("id=\"recommendation-heading\"", content, StringComparison.OrdinalIgnoreCase);

        var zipInput = Regex.Match(
            content,
            "<input[^>]*name=\"ZipCode\"[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.True(zipInput.Success, "The existing ZIP code input was not rendered.");
        Assert.Contains("value=\"45236\"", zipInput.Value, StringComparison.OrdinalIgnoreCase);
    }
}
