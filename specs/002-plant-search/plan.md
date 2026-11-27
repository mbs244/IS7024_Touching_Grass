# Implementation Plan: Plant Search

**Branch**: `plant-search-delriogo` | **Date**: 2026-11-07 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-plant-search/spec.md` and planning constraints supplied by the user.

## Summary

Add a `/Plants` Razor Page that searches the existing plant catalog by partial, case-insensitive common or scientific name and displays up to 50 matches with their water factor. Extend `IPlantRepository` with `SearchAsync`, implement it using the existing `JsonDataLoader`, expose the page from the shared navigation, and test repository behavior with the existing xUnit project. Keep the home page unchanged.

## Technical Context

**Language/Version**: C# on .NET 10

**Primary Dependencies**: ASP.NET Core Razor Pages, Bootstrap assets already in the project, and the existing `JsonDataLoader`; no new packages

**Storage**: Existing bundled `Touching_Grass/Data/plants.json`; no database or API

**Testing**: Existing xUnit project at `tests/Touching_Grass.UnitTests`

**Target Platform**: Existing ASP.NET Core web application

**Project Type**: Server-rendered web application

**Performance Goals**: Return at most 50 matches per search; no additional latency target specified

**Constraints**: Preserve the existing home page; add only shared navigation and a new Plants page. Trim search input, match both names case-insensitively, preserve catalog order, and avoid additional packages, persistence, API, filters, saved lists, autocomplete, and plant detail pages.

**Scale/Scope**: Search the existing bundled plant catalog; one new page, one shared navigation link, and repository/page behavior tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / constraint | Gate | Evidence |
|---|---|---|
| I. Code Quality and Clarity | PASS | Reuse the existing .NET 10 Razor Pages architecture, repository, data loader, and async patterns. |
| II. Testable Delivery | PASS | Add xUnit tests for search matching, normalization, result limit, and empty input; document runnable page acceptance checks. |
| III. Consistent User Experience | PASS | Use the shared layout and Bootstrap styles; provide a labeled form, accessible result/empty-state feedback, and responsive result presentation. |
| IV. Performance by Design | PASS | Bound search results to 50 and reuse the bundled data source without introducing network or database work. |
| V. Maintainable Simplicity | PASS | Add one page and one repository capability; no new dependencies, storage, or service layer. |
| Engineering constraints | PASS | Treat the query as untrusted, trim it, pass cancellation through async loading, and identify the existing local plant data as the source. |

**Post-design re-check**: PASS. The design adds no dependencies or persistence, bounds results, keeps data-load failures distinct from no matches, and provides a labeled search form and semantic result table within the existing page architecture.

## Project Structure

### Documentation (this feature)

```text
specs/002-plant-search/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command; not created by /speckit-plan)
```

### Source Code (repository root)
```text
Touching_Grass/
├── Pages/
│   ├── Plants.cshtml
│   ├── Plants.cshtml.cs
│   └── Shared/_Layout.cshtml          # Add Plants navigation link only
├── Repositories/
│   ├── IPlantRepository.cs            # Add SearchAsync
│   └── PlantRepository.cs             # Search names in existing JSON catalog
└── Data/plants.json                   # Read only; no data changes

tests/Touching_Grass.UnitTests/
└── LocalDataServiceTests.cs           # Extend existing repository tests
```

**Structure Decision**: Extend the existing single ASP.NET Core Razor Pages project and its existing xUnit test project. The shared layout is the only existing view that changes; the home page and plant data remain unchanged.

## Complexity Tracking

No constitution violations or added architectural complexity.
