# Implementation Plan: Authenticated User Accounts and Personal Plants

**Branch**: `002-user-accounts-plants` | **Date**: 2026-11-03 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-user-accounts-plants/spec.md`

## Summary

Add secure local email/password accounts and persistent, user-owned PlantWise personalization to the existing .NET 10 Razor Pages application. Use ASP.NET Core Identity with the Identity Entity Framework store, Entity Framework Core, and SQL Server LocalDB for local development. Each authenticated registered user has one validated default USA ZIP code, a persistent deduplicated list of plants successfully searched through the existing flow, and persistent favorite state.

Saved-plant recommendations reuse the existing deterministic mock-weather inputs and `WateringCalculatorService`. This feature does not add or call Open-Meteo, Visual Crossing, or another external weather provider. Anonymous plant search and recommendation behavior remains unchanged.

## Technical Context

**Language/Version**: C# on .NET 10 with nullable reference types and implicit usings enabled

**Primary Dependencies**: ASP.NET Core Identity, Identity Entity Framework store, Entity Framework Core, SQL Server provider, EF design-time tooling as required for migrations, existing PlantWise repositories and services, Bootstrap/jQuery UI conventions

**Storage**: Relational application database through `ApplicationDbContext`; SQL Server LocalDB for local development. Existing `plants.json` and `zip-locations.json` remain read-only application reference datasets. No account or personal data is stored in JSON.

**Testing**: Reuse the existing `tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj` project already included in `Touching_Grass.slnx`. It targets `net10.0`, uses xUnit with `Microsoft.NET.Test.Sdk` 17.14.1, `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.4, and `coverlet.collector` 6.0.4, and references `Touching_Grass/Touching_Grass.csproj`. Add all new feature tests to this existing project; do not create a duplicate test project or change its target framework unless an approved compatibility decision requires it. Tests include Razor Page/application integration tests, Identity authentication tests, LocalDB-backed persistence tests or the approved isolated test-database strategy, safe-logging tests, and existing watering-calculation regression tests.

**Target Platform**: ASP.NET Core .NET 10 web application using server-rendered Razor Pages

**Project Type**: Single-project server-rendered Razor Pages web application

**Performance Goals**: Apply SC-001 and SC-002 using local development or automated integration tests with LocalDB on the same machine, Release configuration, existing deterministic mock-weather inputs, one warm-up, at least 10 timed executions, request-boundary timing, median and slowest reporting, and no startup/migration/debugger overhead.

**Constraints**: Preserve anonymous lookup behavior; validate ZIPs against the nationwide ZIP dataset; use ASP.NET Core Identity for password storage and verification; use authenticated owner identity for every personal operation; fail safely on unavailable plant or location data; preserve explicit watering units and the established formula; do not add external weather APIs, Identity roles, or unrelated page redesign.

**Scale/Scope**: Two primary user states or personas: anonymous visitor and authenticated registered user. One default ZIP per user, one personal-plant entry per user/source plant, and expected classroom/project-scale account and list volumes. Authorization uses authentication state and ownership, not formal roles.

## Existing Test-Project Decision

- The repository already contains `tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj` and includes it in `Touching_Grass.slnx`.
- The existing project is an SDK-style xUnit test project targeting `net10.0`. It currently uses `Microsoft.NET.Test.Sdk` 17.14.1, `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.4, and `coverlet.collector` 6.0.4, and references `Touching_Grass/Touching_Grass.csproj`.
- Implementation MUST reuse this project and add new feature tests there. It MUST NOT create a duplicate `Touching_Grass.UnitTests` project or change the test target framework unless a separately approved compatibility decision requires it.
- Any additional test dependency must be selected only after inspecting the installed SDK, the application project, and the existing test project. Add ASP.NET Core integration-testing or EF test-database support only when directly required by approved tests.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Code Quality and Clarity**: PASS — concrete account, persistence, repository, service, and Razor Page boundaries reuse existing abstractions and preserve the single calculation path.
- **Testable Delivery**: PASS — task coverage includes Identity, persistence, authorization, privacy, unavailable source plants, mock-weather recommendation parity, safe logging, performance evidence, and anonymous regression tests.
- **Consistent User Experience**: PASS — account, profile, personal-list, empty, unavailable, favorite, and sign-in states use accessible server-rendered Razor Pages controls and messages.
- **Performance by Design**: PASS — scoped owner queries, asynchronous database operations, existing deterministic mock-weather inputs, and defined request-boundary measurements are required.
- **Maintainable Simplicity**: PASS — mutable account data is relational while existing read-only JSON datasets remain unchanged; no second recommendation calculation is introduced, and no external weather provider or live weather API integration is introduced in feature 002. Feature 002 continues to use the existing deterministic mock-weather inputs and the existing `WateringCalculatorService`.
- **Engineering Constraints**: PASS — boundary validation, Identity credential handling, antiforgery-protected POST actions, owner scoping, safe errors, and explicit recommendation units are specified.

## Concrete Project Structure

```text
Touching_Grass/
├── Data/
│   ├── ApplicationDbContext.cs       # Identity and user-plant relational context
│   ├── plants.json                   # Read-only PlantWise reference data
│   └── zip-locations.json            # Read-only ZIP reference data
├── Models/
│   ├── ApplicationUser.cs            # Identity user plus default ZIP
│   ├── UserPlant.cs                  # Owner, source ID/name snapshots, favorite, CreatedAt, LastSearchedAt
│   ├── Plant.cs
│   ├── ZipLocation.cs
│   └── WaterRecommendation.cs
├── Services/
│   ├── IUserPlantService.cs           # Owner-scoped upsert/list/favorite/recommendation boundary
│   ├── UserPlantService.cs
│   ├── IZipLookupService.cs
│   ├── ZipLookupService.cs
│   ├── IPlantRepository.cs
│   ├── PlantRepository.cs
│   ├── IWateringCalculatorService.cs
│   └── WateringCalculatorService.cs  # Existing deterministic mock-weather calculation
├── Pages/
│   ├── Account/
│   │   ├── Register.cshtml(.cs)
│   │   ├── Login.cshtml(.cs)
│   │   ├── Logout.cshtml(.cs)
│   │   └── Profile.cshtml(.cs)
│   ├── MyPlants/
│   │   ├── Index.cshtml(.cs)          # List and OnPostToggleFavoriteAsync
│   │   └── Details.cshtml(.cs)        # Saved-plant recommendation
│   ├── Index.cshtml(.cs)              # Existing anonymous/signed-in search
│   └── Results.cshtml(.cs)            # Existing recommendation presentation
└── Program.cs                         # Identity, EF, auth, authorization, DI

tests/Touching_Grass.UnitTests/
├── IdentityAccountTests.cs
├── DefaultZipTests.cs
├── UserPlantServiceTests.cs
├── CrossUserIsolationTests.cs
├── FavoriteTests.cs
├── UnavailableSourcePlantTests.cs
├── SavedPlantRecommendationTests.cs
├── SafeLoggingAndErrorTests.cs
├── PerformanceMeasurementTests.cs
└── AnonymousPlantWiseRegressionTests.cs
```

## Database and Configuration Decisions

- Inspect the existing application and test projects, confirm both target `net10.0`, inspect the installed SDK and existing package references, and select mutually compatible versions only during implementation; do not invent versions in the plan.
- Use ASP.NET Core Identity and `AddEntityFrameworkStores<ApplicationDbContext>()`.
- Use the SQL Server EF Core provider and LocalDB development connection string held in configuration, not source-controlled secrets.
- Create and apply the initial Entity Framework Core migration from the application project `Touching_Grass/Touching_Grass.csproj`, using the application database context, and store it under `Touching_Grass/Data/Migrations/`. The migration covers ASP.NET Core Identity entities, `ApplicationUser` default ZIP profile data, `UserPlant` ownership and fields, and the unique `(UserId, SourcePlantId)` constraint. The plant and ZIP-location JSON reference datasets remain read-only JSON and are not relational tables. Document normal LocalDB database creation/update instructions.
- Use an isolated SQL Server LocalDB test database with a unique test database name, or another approved SQL Server-compatible relational configuration. Integration tests apply the same application-owned migration to the isolated database; the test project does not own or copy a separate migration set, must not connect to or modify the ordinary developer database, and must begin each execution from a known state without leaked users or test data. Do not silently replace relational integration tests with EF Core InMemory.

### Dependency Boundaries

- **Application project (`Touching_Grass/Touching_Grass.csproj`)**: Select compatible ASP.NET Core Identity Entity Framework store, Entity Framework Core, SQL Server provider, EF Core design-time tooling, and other approved application dependencies after project and SDK inspection. Identity functionality supplied by the shared framework MUST be documented rather than duplicated with an unnecessary package.
- **Existing test project (`tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj`)**: Retain its existing Microsoft.NET.Test.Sdk, xUnit, xUnit runner, and coverlet dependencies. Add only directly required test dependencies, such as ASP.NET Core integration-testing support or EF test-database support, using versions compatible with `net10.0` and the installed SDK. Do not copy application-only packages into the test project without a direct test need.
- Restore and build after any approved dependency additions, resolve conflicts without downgrading `net10.0`, and record selected package names and versions in implementation notes or `quickstart.md`.

### Identity Password Policy Baseline

- Configure the ASP.NET Core Identity password-policy system explicitly with `RequiredLength = 8`, `RequiredUniqueChars = 1`, `RequireDigit = true`, `RequireLowercase = true`, `RequireUppercase = true`, and `RequireNonAlphanumeric = false`.
- Use ASP.NET Core Identity for password hashing, verification, policy enforcement, account lockout behavior when enabled, and safe authentication failures. Do not implement custom password validation, custom hashing, or readable password storage.
- Document the selected values in implementation notes or `quickstart.md`. Registration must provide safe validation messages without echoing or logging password values, hashes, or security stamps; unsuccessful sign-in must use a generic message.

### Migration and Relational Test Database Boundaries

- The application database context is owned by the application project. The only migration location is `Touching_Grass/Data/Migrations/` under `Touching_Grass/Touching_Grass.csproj`; do not create migrations in the test project or a second migration location.
- The initial migration must include ASP.NET Core Identity entities, `ApplicationUser` default ZIP data, `UserPlant` authenticated ownership, `SourcePlantId`, `CommonNameSnapshot`, `ScientificNameSnapshot`, `IsFavorite`, `CreatedAt`, `LastSearchedAt`, foreign-key relationships, and the unique authenticated-user-plus-`SourcePlantId` constraint.
- Integration tests use the application context and application migration against an isolated SQL Server LocalDB database. Each execution creates or resets a known database state, exercises unique constraints, foreign keys, Identity persistence, ownership, duplicate personal-plant prevention, and timestamp persistence, and cannot modify ordinary development or production data.

## Cross-Cutting Rules

- `UserPlant` upsert occurs after successful ZIP resolution, plant resolution, and `WateringCalculatorService` calculation, before the successful response/redirect. The database upsert is atomic but does not require a distributed transaction with read-only JSON/mock-weather operations.
- `UserPlant` retains source ID and common/scientific snapshots but never treats a copied water factor as authoritative.
- `Pages/MyPlants/Index.cshtml.cs` contains the single antiforgery-protected `OnPostToggleFavoriteAsync` handler. It queries by signed-in owner and server-controlled personal-plant ID, persists the toggle, and redirects to the favorites-first list.
- New logging and exception handlers must exclude credentials, hashes, stamps, cookies, connection strings, secrets, antiforgery tokens, full account records, stack traces, database details, and unnecessary profile data.

## Design Artifacts

- `research.md` records Identity, EF/LocalDB, mock-weather, security, and measurement decisions.
- `data-model.md` defines account, source data, retained plant snapshots, ownership, lifecycle, and recommendation invariants.
- `contracts/account-and-plants.md` defines page behavior, upsert timing, favorite handler behavior, and safe error boundaries.
- `quickstart.md` defines end-to-end verification and evidence collection.

## Complexity Tracking

No constitution violations require justification.
