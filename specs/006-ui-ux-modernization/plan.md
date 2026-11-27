# Implementation Plan: UI Consolidation and User Experience Modernization

**Branch**: `[006-ui-ux-modernization]` | **Date**: 2026-11-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/006-ui-ux-modernization/spec.md`

## Summary

Modernize PlantWise UX by consolidating watering input and recommendation display into one landing workflow, adding plant autocomplete, and standardizing naming/branding across Razor Pages without changing domain calculations, authentication behavior, data schema, or external integrations. The design keeps existing services and entities intact while reshaping page composition, title/nav semantics, and user interaction states to improve discoverability and completion rate.

## Technical Context

**Language/Version**: C# with ASP.NET Core Razor Pages on .NET 10 (`net10.0`)

**Primary Dependencies**: ASP.NET Core Razor Pages, ASP.NET Core Identity, Entity Framework Core (InMemory + SqlServer providers), Bootstrap, jQuery validation

**Storage**: Existing ApplicationDbContext-backed identity/profile storage plus existing JSON-backed plant/ZIP datasets; no schema changes

**Testing**: xUnit with ASP.NET Core TestHost and WebApplicationFactory in `tests/Touching_Grass.UnitTests`

**Target Platform**: Web browsers (desktop + mobile), hosted locally and Azure App Service

**Project Type**: Server-rendered web application (single Razor Pages project)

**Performance Goals**: Autocomplete suggestions feel instantaneous for normal dataset size; recommendation flow remains responsive for interactive form submissions

**Constraints**: Preserve existing watering calculation logic, authentication workflow, API/weather integration, and deployment architecture; keep USA ZIP support; maintain keyboard accessibility and responsive UI

**Scale/Scope**: UI consolidation and interaction modernization across existing pages, shared layout, and client-side behavior within one web app and one unit-test project

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Code Quality and Clarity**: PASS — plan reuses existing services/repositories and keeps changes focused on page composition and UX behavior.
- **II. Testable Delivery**: PASS — plan includes unit/integration-host coverage for consolidated flow, titles/nav labels, and autocomplete/validation behaviors.
- **III. Consistent User Experience**: PASS — this feature explicitly standardizes layout, terminology, accessibility, and messaging across pages.
- **IV. Performance by Design**: PASS — autocomplete limit and matching strategy are bounded; no additional external calls introduced.
- **V. Maintainable Simplicity**: PASS — no new backend services or schema, incremental updates within current Razor Pages architecture.

**Post-Design Re-check**: PASS — Phase 1 artifacts keep implementation incremental, measurable, and within existing architecture and constraints.

## Project Structure

### Documentation (this feature)

```text
specs/006-ui-ux-modernization/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── usability-validation-checklist.md
├── contracts/
│   └── landing-workflow-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
Touching_Grass/
├── Pages/
│   ├── Home.cshtml
│   ├── Index.cshtml
│   ├── Index.cshtml.cs
│   ├── Results.cshtml
│   ├── Results.cshtml.cs
│   ├── Plants.cshtml
│   ├── Plants.cshtml.cs
│   └── Shared/
│       └── _Layout.cshtml
├── Services/
├── Repositories/
└── wwwroot/
	├── css/site.css
	└── js/site.js

tests/
└── Touching_Grass.UnitTests/
	├── RazorPagesTestHost.cs
	└── RazorPagesTestHostTests.cs
```

**Structure Decision**: Use the existing single-project Razor Pages structure and unit-test project. No new projects or service boundaries are introduced.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitutional violations identified.
