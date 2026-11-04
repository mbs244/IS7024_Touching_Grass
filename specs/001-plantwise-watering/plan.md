# Implementation Plan: PlantWise Watering Recommendation

**Branch**: `001-plantwise-watering` | **Date**: 2026-11-03 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Implement the first PlantWise vertical slice in the existing .NET 10 ASP.NET Core Razor
Pages project. The slice uses local JSON data and deterministic mock weather values; it
does not call external APIs or require EF Core/LocalDB. The target architecture then
replaces the local data boundary with SQL Server LocalDB and typed Open-Meteo and Visual
Crossing services. In both stages, the calculator applies `max(0, (ET × plant factor) -
rainfall)` and the Razor Page displays a transparent result or safe recovery state.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# on .NET 10 with nullable reference types enabled

**Primary Dependencies**: ASP.NET Core Razor Pages, Entity Framework Core SQL Server
provider, Bootstrap, jQuery, typed `HttpClient` via `IHttpClientFactory`

**Storage**: Current slice uses JSON sample data; target architecture uses SQL Server
LocalDB through EF Core for ZIP, plant, and factor reference data

**Testing**: xUnit with Microsoft.NET.Test.Sdk; unit, EF Core integration, Razor Page
integration, and mocked HTTP contract tests

**Target Platform**: ASP.NET Core web application running locally or on a standard .NET 10
host; desktop browser UI

**Project Type**: Server-rendered Razor Pages web application

**Performance Goals**: At least 95% of valid lookups complete within 10 seconds under
normal network conditions; no unbounded duplicate submissions or outstanding requests

**Constraints**: Current slice makes no external HTTP calls and uses mock weather values;
all stages fail closed on missing required data, validate before provider calls, use
bounded HTTP timeouts and cancellation when providers are introduced, protect credentials,
and use explicit weekly-inch units with decimal calculation arithmetic

**Scale/Scope**: Current slice is one primary lookup page with local reference datasets and
mock weather; the target feature adds two weather providers. Accounts, saved history,
notifications, and recommendation persistence remain out of scope.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Code quality**: PASS — services, repositories, data models, and Razor Page concerns
  remain separated; no duplicate provider or calculation logic is planned.
- **Testable delivery**: PASS — unit, integration, and mocked HTTP contract tests cover
  normal, boundary, invalid, unavailable-data, and provider-failure paths.
- **UX consistency**: PASS — shared Bootstrap validation, loading, result, unavailable,
  and retry states preserve labels, keyboard access, and explicit units.
- **Performance**: PASS — asynchronous database and HTTP I/O, typed client reuse,
  bounded timeouts, cancellation, and a measurable ten-second target are specified.
- **Maintainable simplicity**: PASS — one existing web project and only the requested
  layers/services; no new project split or persistence of recommendation history.
- **Engineering constraints**: PASS — untrusted input is validated, credentials are
  externalized, and unavailable source data never produces a fabricated result.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
Touching_Grass.slnx
├── Touching_Grass/
│   ├── Data/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Pages/
│   └── wwwroot/
└── tests/
	├── Touching_Grass.UnitTests/
	├── Touching_Grass.IntegrationTests/
	└── Touching_Grass.ContractTests/
```

**Structure Decision**: Use the existing single `Touching_Grass` Razor Pages project with
explicit Data, Models, Repositories, Services, Pages, and wwwroot folders. Add focused
test projects only where the current solution has no equivalent. Detailed class
relationships, database schema, API contracts, and test commands are documented in the
Phase 1 artifacts below.

## Complexity Tracking

No constitution violations require justification. The requested repository abstraction is
retained because local plant data access is an explicit feature boundary and must be
independently testable; it does not require an additional project.

## Phase 0 Research Summary

See [research.md](research.md) for decisions on the single-project structure, EF Core and
LocalDB, typed HTTP clients, shared weather normalization, fail-closed behavior, decimal
calculation, and the testing strategy.

## Phase 1 Design Artifacts

- [data-model.md](data-model.md) — entities, SQL schema, relationships, and class diagram.
- [contracts/application.md](contracts/application.md) — PageModel, service, and
  configuration contracts.
- [contracts/open-meteo.md](contracts/open-meteo.md) — Open-Meteo request and normalized
  response contract.
- [contracts/visual-crossing.md](contracts/visual-crossing.md) — Visual Crossing request,
  credentials, and failure contract.
- [quickstart.md](quickstart.md) — solution layout, implementation sequence, setup, and
  testing strategy.

## Post-Design Constitution Check

- **Status**: PASS.
- The design keeps domain calculation explicit, uses asynchronous and bounded I/O,
  protects secrets, includes automated tests for required failure modes, and preserves
  accessible Razor Pages UX states.
