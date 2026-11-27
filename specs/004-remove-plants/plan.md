# Implementation Plan: Remove Plants from My Plants

**Branch**: `remove-plant-delriogo` | **Date**: 2026-11-23 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-remove-plants/spec.md`

## Summary

Add a plant-specific remove action to the authenticated My Plants page. Extend `IUserPlantService` with an owner-scoped removal operation and implement it in both `LocalUserPlantService` and the Azure-persisted `UserPlantService`. On success, use the removed plant's saved name in a confirmation. On a storage error, log the exception, return to My Plants with the specified failure message, and show no success confirmation. Preserve the plant catalog, other users' entries, the home page, and the existing schema.

## Technical Context

**Language/Version**: C# on .NET 10

**Primary Dependencies**: ASP.NET Core Razor Pages, ASP.NET Core Identity, Entity Framework Core, existing `IUserPlantService`; no new packages

**Storage**: Development uses the process-local `LocalUserPlantService`; Azure uses `UserPlantService` with the existing MySQL-backed `ApplicationDbContext`

**Testing**: Existing xUnit project (`tests/Touching_Grass.UnitTests`) with ASP.NET Core `WebApplicationFactory` and EF Core InMemory support

**Target Platform**: Existing ASP.NET Core web application, locally run and Azure-hosted

**Project Type**: Server-rendered Razor Pages web application

**Performance Goals**: Remove one user's selected list entry in one form submission; no additional latency target is specified

**Constraints**: Limit UI changes to My Plants; enforce ownership using the authenticated user's ID and the selected entry ID; do not change the database schema or add migrations; preserve existing favorite behavior and home-page behavior

**Scale/Scope**: One My Plants page, the shared user-plant service contract and its two implementations, and focused tests; no new entities, persistence, pages, or dependencies

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / constraint | Gate | Evidence |
|---|---|---|
| I. Code Quality and Clarity | PASS | Reuse the existing page, `IUserPlantService`, user identity, and service implementations; keep ownership filtering in the service boundary. |
| II. Testable Delivery | PASS | Add tests for both service implementations, owner isolation, missing entries, and authenticated page success and storage-error flows. |
| III. Consistent User Experience | PASS | Keep the action on My Plants, identify each target in its button name, and show success or non-success feedback. |
| IV. Performance by Design | PASS | Operate on one selected user-plant entry and avoid catalog, home-page, or unrelated persistence work. |
| V. Maintainable Simplicity | PASS | Add one service operation and one page action; add no dependencies or data model changes. |
| Engineering constraints | PASS | Derive the user ID from the authenticated principal, honor request cancellation, and make no schema or migration changes. |

**Post-design re-check**: PASS. The design uses the existing local and Azure service registrations, filters every removal by owner and entry ID, logs and reports expected storage errors without reporting success, adds no persistence shape changes, and confines presentation changes to My Plants.

## Project Structure

### Documentation (this feature)

```text
specs/004-remove-plants/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── my-plants-removal.md
└── tasks.md             # Created by /speckit-tasks, not by /speckit-plan
```

### Source Code (repository root)

```text
Touching_Grass/
├── Pages/
│   └── MyPlants/
│       ├── Index.cshtml       # Add the per-plant remove form and feedback
│       └── Index.cshtml.cs    # Add the authenticated remove handler
└── Services/
    ├── IUserPlantService.cs   # Add the owner-scoped removal contract
    ├── LocalUserPlantService.cs
    └── UserPlantService.cs

tests/Touching_Grass.UnitTests/
├── LocalUserPlantServiceTests.cs
├── MyPlantsPageTests.cs       # Add authenticated page-flow coverage
└── UserPlantServiceTests.cs   # Add owner-scoped persistence-service coverage
```

**Structure Decision**: Extend the existing single ASP.NET Core Razor Pages project and xUnit project. Only My Plants presentation files, the existing service abstraction and both implementations, and focused tests are in scope. `UserPlant`, `ApplicationDbContext`, migrations, home-page files, and plant-catalog data remain unchanged.

## Complexity Tracking

No constitution violations or added architectural complexity.
