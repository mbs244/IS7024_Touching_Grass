# Tasks: Plant Search

**Input**: Design documents from `/specs/002-plant-search/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/plants-page.md`, `quickstart.md`

**Tests**: xUnit tests are included because the planning request explicitly requires them.

**Organization**: Tasks are grouped by the prioritized user stories in `spec.md`.

## Phase 1: Setup

**Purpose**: No new project setup is required; the existing .NET 10 app and xUnit project are used.

## Phase 2: Foundational

**Purpose**: Add the repository contract required by both the search implementation and page.

- [X] T001 Add `SearchAsync(string searchTerm, CancellationToken cancellationToken = default)` returning `Task<IReadOnlyList<Plant>>` to `Touching_Grass/Repositories/IPlantRepository.cs`.

## Phase 3: User Story 1 - Find plants by name (Priority: P1) 🎯 MVP

**Goal**: Search supported plants by partial common or scientific name and display up to 50 matching plants with their water factor.

**Independent Test**: Search `salvia` and confirm white sage appears; search `TOYON` and confirm toyon appears. Verify each result includes common name, scientific name, and water factor, and that no more than 50 results appear in catalog order.

### Tests for User Story 1

- [X] T002 [US1] Extend the plant fixture and add xUnit coverage for partial common/scientific-name matches, case-insensitive comparison, whitespace trimming, an empty term, and the 50-result catalog-order limit in `tests/Touching_Grass.UnitTests/LocalDataServiceTests.cs`.

### Implementation for User Story 1

- [X] T003 [US1] Implement `SearchAsync` in `Touching_Grass/Repositories/PlantRepository.cs` using `JsonDataLoader` to match either name with ordinal case-insensitive substring comparison, preserve catalog order, return at most 50 records, and return no results for an empty trimmed term without loading the catalog.
- [X] T004 [P] [US1] Create `PlantsModel` in `Touching_Grass/Pages/Plants.cshtml.cs` with a GET-bound search term, cancellation-aware repository call, and result state for the Plants page.
- [X] T005 [P] [US1] Create `Touching_Grass/Pages/Plants.cshtml` with a labeled GET search form and a semantic, captioned results table showing common name, scientific name, and water factor.
- [X] T006 [P] [US1] Add a Plants navigation link to `Touching_Grass/Pages/Shared/_Layout.cshtml` without changing `Touching_Grass/Pages/Index.cshtml` or `Touching_Grass/Pages/Index.cshtml.cs`.

**Checkpoint**: The name-search flow is usable from navigation and displays the requested fields for matching plants.

## Phase 4: User Story 2 - Understand when a plant is not found (Priority: P2)

**Goal**: Clearly distinguish no matches, an empty search, and unavailable catalog data.

**Independent Test**: Search `sklujapouetllkjsda`; verify zero plant rows and the exact message `This plant is not in our supported plant database.` Confirm an empty search shows no no-match message.

### Tests for User Story 2

- [X] T007 [US2] Add xUnit tests for the Plants page model's empty-query and no-match states, exact not-found message, and separate catalog-load failure state in `tests/Touching_Grass.UnitTests/PlantsPageTests.cs`.

### Implementation for User Story 2

- [X] T008 [US2] Update `Touching_Grass/Pages/Plants.cshtml.cs` to distinguish an empty query from a non-empty query with no results, expose the exact required no-match message, and log catalog-load failures while setting a separate user-friendly unavailable-data state.
- [X] T009 [US2] Update `Touching_Grass/Pages/Plants.cshtml` to show the exact not-found message only after a non-empty no-match search and to display catalog-unavailable feedback separately from no matches.

**Checkpoint**: A no-match search, an empty query, and a catalog failure each produce their specified distinct page state.

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Verify the full feature and ensure existing behavior is preserved.

- [X] T010 Run the existing xUnit suite with `dotnet test .\tests\Touching_Grass.UnitTests\Touching_Grass.UnitTests.csproj` and manually validate the scenarios in `specs/002-plant-search/quickstart.md`, including the unchanged home page.

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No setup tasks are needed; the application and test projects already exist.
- **Foundational (Phase 2)**: T001 adds the repository contract and must complete before feature implementation.
- **User Story 1 (Phase 3)**: Depends on T001. T002 should be written before T003 so the repository behavior is test-first. T004, T005, and T006 touch separate files and can proceed in parallel after T001; T005 relies on the page model contract established in T004.
- **User Story 2 (Phase 4)**: Depends on the search page from User Story 1. T007 tests the page-state behavior before T008 and T009 implement it; T009 relies on the state exposed by T008.
- **Polish (Phase 5)**: Depends on both story phases being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Can start after T001; no dependency on another story.
- **User Story 2 (P2)**: Depends on User Story 1's page and search results so the no-match outcome can be represented distinctly from successful matches.

### Parallel Opportunities

- After T001, T004 and T006 can be implemented in parallel because they modify different files.
- After T004 defines the page model properties, T005 can be implemented independently of the repository implementation in T003.
- Within User Story 2, test design in T007 can proceed while the UI shell for User Story 1 is completed; implementation T008 precedes T009.

## Parallel Example: User Story 1

```text
After T001:
Task: Create PlantsModel in Touching_Grass/Pages/Plants.cshtml.cs (T004)
Task: Add the Plants link in Touching_Grass/Pages/Shared/_Layout.cshtml (T006)

After T004 establishes the page model contract:
Task: Create the search form and results table in Touching_Grass/Pages/Plants.cshtml (T005)
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001 to establish the repository contract.
2. Complete T002 and T003 to test and implement search behavior.
3. Complete T004, T005, and T006 to expose the search through the page and navigation.
4. Run the targeted tests and verify the `salvia` and `TOYON` scenarios.

### Incremental Delivery

1. Deliver User Story 1 as the searchable plant catalog.
2. Add User Story 2's distinct empty, no-match, and unavailable-data feedback.
3. Run the full xUnit suite and validate all scenarios in `quickstart.md`.

## Notes

- `[P]` tasks operate on separate files and have no unresolved task dependency.
- Every task includes a repository-relative file path.
- No package installation, database, external API, plant-data edit, or home-page change is planned.
