# PlantWise Implementation Quickstart

## Solution structure

The existing solution remains a single web project targeting .NET 10:

```text
Touching_Grass.slnx
└── Touching_Grass/
	├── Data/
	│   ├── ApplicationDbContext.cs
	│   ├── DbInitializer.cs
	│   └── Migrations/
	├── Models/
	│   ├── Plant.cs
	│   ├── PlantWaterFactor.cs
	│   ├── ZipLocation.cs
	│   ├── WeatherObservation.cs
	│   └── WateringRecommendation.cs
	├── Repositories/
	│   ├── IPlantRepository.cs
	│   └── PlantRepository.cs
	├── Services/
	│   ├── IZipLookupService.cs
	│   ├── ZipLookupService.cs
	│   ├── IOpenMeteoService.cs
	│   ├── OpenMeteoService.cs
	│   ├── IVisualCrossingService.cs
	│   ├── VisualCrossingService.cs
	│   ├── IWateringCalculatorService.cs
	│   └── WateringCalculatorService.cs
	├── Pages/
	│   ├── Index.cshtml
	│   ├── Index.cshtml.cs
	│   └── Shared/
	├── wwwroot/
	│   ├── css/site.css
	│   └── js/site.js
	├── Program.cs
	└── appsettings*.json
└── tests/
	├── Touching_Grass.UnitTests/
	├── Touching_Grass.IntegrationTests/
	└── Touching_Grass.ContractTests/
```

The repository currently has one application project. Add test projects only if the
solution's current test setup does not already provide them.

## Implementation sequence

1. Add EF Core SQL Server and tooling packages compatible with .NET 10.
2. Add entities, `ApplicationDbContext`, constraints, migration, and repeatable seed/import
   for ZIP and plant data.
3. Implement `PlantRepository` and `ZipLookupService` with normalized, asynchronous,
   no-tracking reads.
4. Implement typed HTTP clients for Open-Meteo and Visual Crossing with configured
   timeouts, cancellation, response validation, unit normalization, and safe logging.
5. Implement `WateringCalculatorService` with decimal arithmetic:
   `gross = etInches * plantFactor`; `net = max(0, gross - rainfallInches)`.
6. Update the Razor Page and Bootstrap view model for validation, loading, result,
   unavailable, and retry states. Use minimal jQuery only for duplicate-submit prevention
   and presentation behavior; server-side validation remains authoritative.
7. Register services, options, DbContext, and logging in `Program.cs`.
8. Add automated tests and run the build, migrations validation, and test suite.

## Local setup

```powershell
dotnet restore
dotnet user-secrets init --project .\Touching_Grass\Touching_Grass.csproj
dotnet user-secrets set "Weather:VisualCrossing:ApiKey" "<local-key>" --project .\Touching_Grass\Touching_Grass.csproj
dotnet ef database update --project .\Touching_Grass\Touching_Grass.csproj
dotnet run --project .\Touching_Grass\Touching_Grass.csproj
```

The connection string and provider base URLs should be configured through local
configuration. Do not commit the API key.

## Testing strategy

### Unit tests

- ZIP trimming, five-digit validation, unsupported ZIP, missing coordinates, and timezone.
- Case-insensitive plant matching, missing match, ambiguous match, and factor range.
- Weekly aggregation, inches conversion, same-period enforcement, and invalid provider values.
- Watering formula with zero values, rainfall greater than gross need, exact zero, and
  decimal precision; verify negative output is clamped.
- PageModel mapping for validation, unavailable, recommendation, and retry states.

### Integration tests

- EF Core repository lookups against a test database using schema constraints and seeded
  representative rows.
- Razor Page POST with valid and invalid form values; verify validation messages and that
  invalid input does not invoke weather services.
- Service orchestration with stubbed provider services; verify partial provider failure
  produces no recommendation.

### HTTP contract tests

- Mock `HttpMessageHandler` responses for Open-Meteo and Visual Crossing success,
  timeout, cancellation, rate limit, non-success status, malformed JSON, missing fields,
  negative values, and unit conversion.
- Verify request parameters include coordinates, local timezone, and the same seven-day
  date range.
- Verify API keys are sent only through the configured provider request and never appear
  in logs or UI messages.

### Quality gates

- `dotnet build` succeeds with no new warnings.
- All tests pass without live API calls.
- Coverage includes normal, boundary, invalid, unavailable-data, and API-failure cases.
- Manual accessibility review confirms labels, validation summary, keyboard flow, and
  responsive Bootstrap layout.
- A local timing check demonstrates the specified valid lookup target of 10 seconds under
  normal network conditions.
