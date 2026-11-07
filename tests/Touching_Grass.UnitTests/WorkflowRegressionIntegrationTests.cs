using System.Text.RegularExpressions;

namespace Touching_Grass.UnitTests;

public sealed class WorkflowRegressionIntegrationTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public WorkflowRegressionIntegrationTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task ValidExistingFormSubmissionRedirectsToResultsWithRecommendation()
    {
        var formResponse = await _host.Client.GetAsync("/Index");
        var formContent = await formResponse.Content.ReadAsStringAsync();
        formResponse.EnsureSuccessStatusCode();
        var antiforgeryCookie = formResponse.Headers.GetValues("Set-Cookie")
            .Select(cookie => cookie.Split(';', 2)[0])
            .Single(cookie => cookie.StartsWith(".AspNetCore.Antiforgery.", StringComparison.Ordinal));

        var tokenMatch = Regex.Match(
            formContent,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.True(tokenMatch.Success, "The existing watering form antiforgery token was not rendered.");

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = tokenMatch.Groups[1].Value,
            ["ZipCode"] = "45236",
            ["PlantName"] = "Toyon"
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/Index")
        {
            Content = form
        };
        request.Headers.Add("Cookie", antiforgeryCookie);

        using var response = await _host.Client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Results", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }
}
