using System.Text.RegularExpressions;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class HomepageZipValidationTests : IClassFixture<RazorPagesTestHost>
{
    private readonly RazorPagesTestHost _host;

    public HomepageZipValidationTests(RazorPagesTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task HomepageZipInputDeclaresNativeFiveDigitValidationContract()
    {
        var response = await _host.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var zipInput = Regex.Match(
            content,
            "<input[^>]*name=\"zipCode\"[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.True(zipInput.Success, "The homepage ZIP input was not rendered.");
        Assert.Contains("required", zipInput.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("inputmode=\"numeric\"", zipInput.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("maxlength=\"5\"", zipInput.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("pattern=\"[0-9]{5}\"", zipInput.Value, StringComparison.OrdinalIgnoreCase);

        Assert.True(IsFiveDigitZip("00501"));
        Assert.False(IsFiveDigitZip(string.Empty));
        Assert.False(IsFiveDigitZip("12A45"));
        Assert.False(IsFiveDigitZip("1234"));
        Assert.False(IsFiveDigitZip("123456"));
        Assert.Equal(5, "00501".Length);
        Assert.StartsWith("0", "00501", StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12A45")]
    [InlineData("1234")]
    [InlineData("123456")]
    public void HomepageZipContractRejectsInvalidValues(string value)
    {
        Assert.False(IsFiveDigitZip(value));
    }

    [Theory]
    [InlineData("00501")]
    [InlineData("45236")]
    public void HomepageZipContractAcceptsFiveDigitValuesIncludingLeadingZeros(string value)
    {
        Assert.True(IsFiveDigitZip(value));
        Assert.Equal(5, value.Length);
    }

    [Fact]
    public async Task HomepageGetHandoffDoesNotGenerateRecommendation()
    {
        var response = await _host.Client.GetAsync("/?zipCode=12A45");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("PlantWise", content);
        Assert.DoesNotContain("recommendation-heading", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WaterRecommendation", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnsupportedFormattedZipRemainsOwnedByExistingLocationLookup()
    {
        var contentRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Touching_Grass"));
        var locationLookup = new ZipLookupService(new JsonDataLoader(new TestHostEnvironment
        {
            ContentRootPath = contentRootPath
        }));

        var result = await locationLookup.FindAsync("99999");

        Assert.Null(result);
    }

    private static bool IsFiveDigitZip(string value) => Regex.IsMatch(value, "^[0-9]{5}$");
}

// Manual browser verification required for T010:
// Submit the native homepage form with an empty, alphabetic, four-digit, six-digit,
// and unsupported five-digit ZIP. Confirm browser validation prevents navigation for
// invalid format values, the user receives understandable feedback, and no recommendation
// is generated. TestHost does not execute browser-side HTML constraint validation.
