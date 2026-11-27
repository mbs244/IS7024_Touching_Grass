using System.Net;
using System.Text.RegularExpressions;

namespace Touching_Grass.UnitTests;

public sealed class IndexPageWorkflowTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public IndexPageWorkflowTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task PostingValidInput_RendersRecommendationOnSamePage()
    {
        var getResponse = await _host.Client.GetAsync("/");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        getResponse.EnsureSuccessStatusCode();

        var antiforgeryCookie = getResponse.Headers.GetValues("Set-Cookie")
            .Select(cookie => cookie.Split(';', 2)[0])
            .Single(cookie => cookie.StartsWith(".AspNetCore.Antiforgery.", StringComparison.Ordinal));
        var token = ExtractToken(getContent);

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["ZipCode"] = "45236",
            ["PlantName"] = "Toyon"
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/") { Content = form };
        request.Headers.Add("Cookie", antiforgeryCookie);

        var postResponse = await _host.Client.SendAsync(request);
        var postContent = await postResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.Contains("id=\"recommendation-heading\"", postContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("How this was calculated", postContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("The recommendation is no longer available", postContent, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractToken(string html)
    {
        var tokenMatch = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.True(tokenMatch.Success, "The anti-forgery token was not found.");
        return tokenMatch.Groups[1].Value;
    }
}
