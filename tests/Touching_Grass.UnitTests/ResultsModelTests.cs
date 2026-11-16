using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Touching_Grass.Models;
using Touching_Grass.Pages;
using Touching_Grass.Services;

namespace Touching_Grass.UnitTests;

public sealed class ResultsModelTests
{
    [Fact]
    public void OnGetPassesRecommendationNeedToStatusService()
    {
        var service = new RecordingWaterStatusMessageService();
        var model = CreateModel(service, new WaterRecommendation
        {
            NetNeedInches = 0.14m
        });

        model.OnGet();

        Assert.Equal(0.14m, service.ReceivedNeed);
        Assert.Equal("Water lightly this week: about 0.14 in.", model.WaterStatusMessage?.Message);
        Assert.NotNull(model.Recommendation);
    }

    [Fact]
    public void OnGetDoesNotExposeRecommendationStatusForNegativeNeed()
    {
        var model = CreateModel(new WaterStatusMessageService(), new WaterRecommendation
        {
            NetNeedInches = -0.01m
        });

        model.OnGet();

        Assert.Null(model.WaterStatusMessage);
        Assert.Null(model.Recommendation);
        Assert.NotNull(model.ErrorMessage);
    }

    private static ResultsModel CreateModel(IWaterStatusMessageService statusService, WaterRecommendation recommendation)
    {
        var tempData = new Dictionary<string, object>
        {
            ["WaterRecommendation"] = JsonSerializer.Serialize(recommendation)
        };
        var model = new ResultsModel(NullLogger<ResultsModel>.Instance, statusService)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider(tempData))
        };
        return model;
    }

    private sealed class TestTempDataProvider(IDictionary<string, object> values) : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => values;

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }

    private sealed class RecordingWaterStatusMessageService : IWaterStatusMessageService
    {
        private readonly WaterStatusMessageService _inner = new();

        public decimal ReceivedNeed { get; private set; }

        public WaterStatusMessage Create(decimal netNeedInches)
        {
            ReceivedNeed = netNeedInches;
            return _inner.Create(netNeedInches);
        }
    }
}
