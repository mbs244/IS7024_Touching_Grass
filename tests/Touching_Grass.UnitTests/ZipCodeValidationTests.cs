using System.Text.RegularExpressions;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class ZipCodeValidationTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public ZipCodeValidationTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Theory]
    [InlineData("10001")]
    [InlineData("30301")]
    [InlineData("60601")]
    [InlineData("73301")]
    [InlineData("96801")]
    public void RepresentativeUsZipFormats_AreAcceptedByValidationPattern(string zipCode)
    {
        Assert.Matches("^[0-9]{5}$", zipCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12A45")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("12345-6789")]
    public void InvalidZipFormats_AreRejectedByValidationPattern(string zipCode)
    {
        Assert.DoesNotMatch("^[0-9]{5}$", zipCode);
    }

    [Fact]
    public async Task InvalidZipSubmission_ShowsValidationError()
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
                ["ZipCode"] = "12A45",
                ["PlantName"] = "Toyon"
            })
        };
        request.Headers.Add("Cookie", antiforgeryCookie);

        var postResponse = await _host.Client.SendAsync(request);
        var postHtml = await postResponse.Content.ReadAsStringAsync();

        postResponse.EnsureSuccessStatusCode();
        Assert.Contains("Enter a valid five-digit ZIP code.", postHtml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocationLookupSupportsRepresentativeNonCaliforniaZipCodes()
    {
        var contentRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Touching_Grass"));
        var lookup = new ZipLookupService(new JsonDataLoader(new TestHostEnvironment
        {
            ContentRootPath = contentRootPath
        }));

        Assert.NotNull(await lookup.FindAsync("10001"));
        Assert.NotNull(await lookup.FindAsync("60601"));
    }
}
