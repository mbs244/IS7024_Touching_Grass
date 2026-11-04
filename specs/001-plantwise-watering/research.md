# PlantWise Technical Research

## Decision: Use a single Razor Pages web project with explicit application layers

- **Decision**: Keep the existing `Touching_Grass` ASP.NET Core web project and organize
  feature code into `Models`, `Data`, `Repositories`, `Services`, and `Pages` folders.
- **Rationale**: The repository currently contains one .NET 10 Razor Pages project. A
  single-project layered design satisfies the requested architecture without adding
  project boundaries or deployment complexity.
- **Alternatives considered**: Separate domain, infrastructure, and web projects were
  rejected for this feature because the current application is small and the constitution
  requires maintainable simplicity.

## Target Decision: Use EF Core with SQL Server LocalDB for local reference data

- **Decision**: Store ZIP locations, plants, and plant factors in EF Core entities and
  seed/import them into SQL Server LocalDB.
- **Rationale**: Local lookup data remains available without a network dependency and is
  queryable through the required repository abstraction.
- **Alternatives considered**: Reading CSV files on every request was rejected because it
  adds repeated parsing work and weakens query/test boundaries. A hosted production
  database is outside this feature's local-development scope.

## Current Slice Decision: Use JSON sample data temporarily

- **Decision**: The first vertical slice reads ZIP locations and plants from local JSON
  files and does not initialize EF Core, LocalDB, or external weather clients.
- **Rationale**: This produces a complete demonstrable user journey with deterministic
  mock weather values while keeping the integration boundary ready for the target design.
- **Migration**: The deferred EF Core task replaces the JSON repository implementation
  after the slice is accepted; the user-facing contracts remain unchanged.

## Decision: Use typed service boundaries for weather providers

- **Decision**: Implement `OpenMeteoService` and `VisualCrossingService` behind focused
  interfaces and register each with `IHttpClientFactory`-managed `HttpClient` instances.
- **Rationale**: Typed clients provide safe client reuse, configurable timeouts, testable
  HTTP boundaries, and a place to normalize provider payloads to weekly inches.
- **Alternatives considered**: Direct `HttpClient` construction in the PageModel was
  rejected because it violates separation of concerns and makes failure tests harder.

## Decision: Normalize both providers to a shared weekly weather contract

- **Decision**: Request seven complete calendar days in the resolved location's local
  timezone, sum daily values, normalize to inches, and expose source, period, and unit in
  the application result.
- **Rationale**: The specification requires aligned weekly totals and transparent results.
  The shared contract prevents provider-specific units from reaching the calculator.
- **Alternatives considered**: Using provider-specific weekly totals was rejected because
  the values could use different periods or aggregation rules.

## Decision: Fail closed when required data is unavailable

- **Decision**: Return a typed unavailable-data outcome when ZIP coordinates, plant data,
  either weather value, credentials, or payload validation is unavailable. Do not calculate
  from partial data or silently substitute zero.
- **Rationale**: The constitution requires explicit units and no uncertain estimate when
  source data is missing. Provider details remain in structured logs, not UI messages.
- **Alternatives considered**: Partial recommendations and fallback zeros were rejected
  because they can mislead users about watering needs.

## Decision: Calculate and round only at presentation

- **Decision**: Calculate with decimal values in inches, clamp negative net need to zero,
  and round only the displayed amount using one documented presentation precision.
- **Rationale**: Delaying rounding avoids cumulative precision loss and makes calculation
  tests deterministic. The selected precision must be applied consistently in the result,
  explanation, and tests.
- **Alternatives considered**: Rounding each provider value before calculation was
  rejected because it changes the mathematical result.

## Decision: Use deterministic tests at three levels

- **Decision**: Use unit tests for validation and calculation, integration tests with an
  in-memory or LocalDB database for repositories and Razor Pages, and HTTP contract tests
  with mocked handlers for both providers.
- **Rationale**: This covers the constitution's required normal, boundary, invalid, and
  unavailable-data cases while avoiding live API dependency in CI.
- **Alternatives considered**: Live provider calls in automated tests were rejected because
  rate limits, credentials, and changing weather data make tests unreliable.
