namespace Touching_Grass.UnitTests;

public sealed class RazorPagesTestHostTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public RazorPagesTestHostTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task HostsExistingIndexPage()
    {
        var response = await _host.Client.GetAsync("/Index");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("id=\"watering-form\"", content);
        Assert.Contains("name=\"ZipCode\"", content);
        Assert.Contains("name=\"PlantName\"", content);
    }
}
