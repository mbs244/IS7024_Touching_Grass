using System.Text.RegularExpressions;

namespace Touching_Grass.UnitTests;

public sealed class NonCaliforniaZipWorkflowTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public NonCaliforniaZipWorkflowTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Theory]
    [InlineData("10001")]
    [InlineData("60601")]
    public async Task WateringGuideWorkflow_WorksForRepresentativeNonCaliforniaZips(string zipCode)
    {
        var getResponse = await _host.Client.GetAsync("/");
        var html = await getResponse.Content.ReadAsStringAsync();
        getResponse.EnsureSuccessStatusCode();

        var token = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.IgnoreCase).Groups[1].Value;
        var antiforgeryCookie = getResponse.Headers.GetValues("Set-Cookie")
            .Select(cookie => cookie.Split(';', 2)[0])
            .Single(cookie => cookie.StartsWith(".AspNetCore.Antiforgery.", StringComparison.Ordinal));

        using var request = new HttpRequestMessage(HttpMethod.Post, "/")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["ZipCode"] = zipCode,
                ["PlantName"] = "Toyon"
            })
        };
        request.Headers.Add("Cookie", antiforgeryCookie);

        var response = await _host.Client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("id=\"recommendation-heading\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"ZIP code: {zipCode}", content, StringComparison.OrdinalIgnoreCase);
    }
}
