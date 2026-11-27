# Quickstart: Plain-Language Watering Status

## Prerequisites

- .NET 10 SDK
- Repository restored (no new package dependencies are required)

## Automated validation

From the repository root, run:

```powershell
dotnet test tests\Touching_Grass.UnitTests\Touching_Grass.UnitTests.csproj
dotnet build Touching_Grass\Touching_Grass.csproj
```

Expected results: all existing and new xUnit tests pass, and the web project builds without errors.

The `WaterStatusMessageServiceTests` should cover these observable outcomes:

| Net need input (in) | Expected result |
|---:|---|
| `0.00` | `No extra watering needed this week.` |
| `0.004` | Rounded to `0.00`; no extra watering |
| `0.005` | `Water lightly this week: about 0.01 in.` |
| `0.244` | `Water lightly this week: about 0.24 in.` |
| `0.245` | `Water about 0.25 in this week.` |
| Negative | Rejected with `ArgumentOutOfRangeException` |

`ResultsModelTests` should verify the service's message is made available to the page and that a rejected negative need does not produce a valid recommendation status. Also verify that service registration is wired and that nonzero amounts always have exactly two decimal places and use the `in` unit.

## Manual end-to-end check

1. Start the application from the repository root:

   ```powershell
   dotnet run --project Touching_Grass\Touching_Grass.csproj
   ```

2. Open the local application URL printed by the command.
3. Submit the lookup form with a supported five-digit U.S. ZIP code and a plant name present in the application's plant data.
4. On the Results page, verify that the weekly status is visible, its phrase matches the rounded recommendation, and the rendered message is in a `role="status"` region that assistive technology can identify as a status update.
5. Confirm that the existing watering calculation and its explanatory values remain unchanged.

For the data definition and invalid-input behavior, see [data-model.md](data-model.md). No external API contract is added by this internal Razor Pages feature.
