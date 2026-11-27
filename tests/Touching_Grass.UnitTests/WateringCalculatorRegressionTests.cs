using System.Text.RegularExpressions;

namespace Touching_Grass.UnitTests;

public sealed class WateringCalculatorRegressionTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public WateringCalculatorRegressionTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task SameInput_ProducesStableDisplayedRecommendation()
    {
        var first = await SubmitAsync("45236", "Toyon");
        var second = await SubmitAsync("45236", "Toyon");

        var firstNeed = ExtractNetNeed(first);
        var secondNeed = ExtractNetNeed(second);

        Assert.Equal(firstNeed, secondNeed);
    }

    private async Task<string> SubmitAsync(string zipCode, string plantName)
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
            ["ZipCode"] = zipCode,
            ["PlantName"] = plantName
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/") { Content = form };
        request.Headers.Add("Cookie", antiforgeryCookie);

        var response = await _host.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
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

    private static string ExtractNetNeed(string html)
    {
        var match = Regex.Match(html, "<p class=\"display-6\">([^<]+)</p>", RegexOptions.IgnoreCase);
        Assert.True(match.Success, "Net watering need display value was not found.");
        return match.Groups[1].Value.Trim();
    }
}
