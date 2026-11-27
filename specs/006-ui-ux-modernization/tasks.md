# Tasks: UI Consolidation and User Experience Modernization

**Input**: Design documents from `/specs/006-ui-ux-modernization/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Include automated tests per constitution requirements for behavior changes.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare shared UI/testing scaffolding for implementation.

- [X] T001 Review and align route ownership between `Touching_Grass/Pages/Home.cshtml`, `Touching_Grass/Pages/Index.cshtml`, and `Touching_Grass/Pages/Index.cshtml.cs` for unified landing workflow.
- [X] T002 [P] Add/adjust shared styles needed for cross-page Plantwise consistency in `Touching_Grass/wwwroot/css/site.css`.
- [X] T003 [P] Add/adjust shared client behavior entry points (autocomplete and in-page result behavior) in `Touching_Grass/wwwroot/js/site.js`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core plumbing required before user-story implementation.

**CRITICAL**: No user story work begins until this phase is complete.

- [X] T004 Add landing-page interaction model properties and handler structure (including recommendation display model support) in `Touching_Grass/Pages/Index.cshtml.cs`.
- [X] T005 [P] Add plant suggestion DTO/model for autocomplete handler responses in `Touching_Grass/Models/PlantSuggestionOption.cs`.
- [X] T006 Implement autocomplete page handler contract (3+ chars, case-insensitive contains, max 10, no-results messaging) in `Touching_Grass/Pages/Index.cshtml.cs`.
- [X] T007 Add helper/service usage updates for identifier-safe plant selection mapping in `Touching_Grass/Pages/Index.cshtml.cs` and `Touching_Grass/Repositories/IPlantRepository.cs` / `Touching_Grass/Repositories/PlantRepository.cs` only if required.
- [X] T008 Establish title and navigation naming baseline (Plantwise prefix and user-facing labels) in `Touching_Grass/Pages/Shared/_Layout.cshtml` and page-level `ViewData["Title"]` assignments.

**Checkpoint**: Foundation ready for independent user-story execution.

---

## Phase 3: User Story 1 - Unified Landing Watering Guide (Priority: P1) MVP

**Goal**: Users can enter ZIP and plant on the landing page and view watering recommendation in the same workflow without a separate results workflow page.

**Independent Test**: From `/`, submit valid ZIP + plant and verify recommendation content appears in-page with no separate workflow redirect.

### Tests for User Story 1

- [X] T009 [P] [US1] Add Razor Page host test for unified landing workflow rendering in `tests/Touching_Grass.UnitTests/RazorPagesTestHostTests.cs`.
- [X] T010 [P] [US1] Add integration-style page test for in-page recommendation display (no separate workflow redirect) in `tests/Touching_Grass.UnitTests/IndexPageWorkflowTests.cs`.
- [X] T011 [US1] Add regression test for unchanged recommendation calculation output on same inputs in `tests/Touching_Grass.UnitTests/WateringCalculatorRegressionTests.cs`.
- [X] T036 [P] [US1] Add anonymous-user test verifying ZIP input is empty on initial load in `tests/Touching_Grass.UnitTests/IndexPageAuthenticationTests.cs` (FR-007).
- [X] T037 [P] [US1] Add authentication-boundary tests verifying ZIP autofill runs only for authenticated users and that no profile lookup occurs when unauthenticated in `tests/Touching_Grass.UnitTests/IndexPageAuthenticationTests.cs` (FR-007).
- [X] T042 [P] [US1] Add parameterized ZIP acceptance tests for representative U.S. ZIPs `10001`, `30301`, `60601`, `73301`, and `96801` in `tests/Touching_Grass.UnitTests/ZipCodeValidationTests.cs` (FR-020).
- [X] T043 [P] [US1] Add invalid ZIP format rejection tests (non-numeric, wrong length, extended formats) in `tests/Touching_Grass.UnitTests/ZipCodeValidationTests.cs` (FR-020).
- [X] T044 [US1] Add end-to-end integration test verifying watering-guide workflow succeeds for non-California ZIPs in `tests/Touching_Grass.UnitTests/NonCaliforniaZipWorkflowTests.cs` (FR-020).

### Implementation for User Story 1

- [X] T012 [US1] Consolidate landing and watering form UX into `Touching_Grass/Pages/Index.cshtml` using single primary tagline and calculator form.
- [X] T013 [US1] Update `Touching_Grass/Pages/Index.cshtml.cs` to render recommendation results in the landing flow and stop depending on separate workflow navigation.
- [X] T014 [US1] Remove redundant landing-to-guide collection workflow from `Touching_Grass/Pages/Home.cshtml` (or repurpose route to unified landing shell).
- [X] T015 [US1] Decommission or simplify separate workflow presentation in `Touching_Grass/Pages/Results.cshtml` and `Touching_Grass/Pages/Results.cshtml.cs` to avoid duplicate user path.
- [X] T016 [US1] Ensure authenticated ZIP autofill behavior remains intact in `Touching_Grass/Pages/Index.cshtml.cs`.
- [X] T038 [US1] Add explicit guard in `Touching_Grass/Pages/Index.cshtml.cs` to ensure profile lookup and ZIP autofill logic execute only for authenticated users (FR-007).
- [X] T045 [US1] Confirm ZIP validation and lookup behavior in `Touching_Grass/Pages/Index.cshtml.cs` and `Touching_Grass/Services/ZipLookupService.cs` accepts valid U.S. ZIPs and rejects invalid formats without state-specific constraints (FR-020).

**Checkpoint**: User Story 1 delivers the MVP independently.

---

## Phase 4: User Story 2 - Plant Autocomplete and Selection Accuracy (Priority: P2)

**Goal**: Users receive fast, accurate plant suggestions while typing and can select the exact identifier used by existing watering calculations.

**Independent Test**: Type 3+ characters (e.g., `Maple`), verify up to 10 case-insensitive contains suggestions with common+scientific names, select one suggestion, then calculate successfully.

### Tests for User Story 2

- [X] T017 [P] [US2] Add handler-level test for 3-character threshold and case-insensitive contains matching in `tests/Touching_Grass.UnitTests/PlantAutocompleteHandlerTests.cs`.
- [X] T018 [P] [US2] Add test for max-10 suggestion cap and no-results response in `tests/Touching_Grass.UnitTests/PlantAutocompleteHandlerTests.cs`.
- [X] T019 [US2] Add interaction test validating selected suggestion maps to exact plant identifier used for calculation in `tests/Touching_Grass.UnitTests/PlantAutocompleteInteractionTests.cs`.

### Implementation for User Story 2

- [X] T020 [US2] Add autocomplete UI elements and accessible suggestion list behavior in `Touching_Grass/Pages/Index.cshtml`.
- [X] T021 [US2] Implement client-side autocomplete request/selection logic in `Touching_Grass/wwwroot/js/site.js`.
- [X] T022 [US2] Finalize autocomplete server handler behavior and response payload in `Touching_Grass/Pages/Index.cshtml.cs`.
- [X] T023 [US2] Ensure no-results and unavailable-data messages are user-friendly and consistent in `Touching_Grass/Pages/Index.cshtml` and `Touching_Grass/Pages/Index.cshtml.cs`.

**Checkpoint**: User Story 2 is independently functional and testable.

---

## Phase 5: User Story 3 - Consistent Plantwise Design, Titles, and Navigation (Priority: P3)

**Goal**: All pages present cohesive Plantwise styling, user-friendly navigation labels, and standardized browser titles.

**Independent Test**: Navigate key pages and verify consistent style patterns, `Plantwise`-prefixed titles, no `Touching_Grass` title text, and `Plant Search` navigation labeling.

### Tests for User Story 3

- [X] T024 [P] [US3] Add test coverage for standardized page title rendering and forbidden `Touching_Grass` title text in `tests/Touching_Grass.UnitTests/NavigationAndTitleConsistencyTests.cs`.
- [X] T025 [P] [US3] Add test coverage for updated navigation labels, including `Plant Search`, in `tests/Touching_Grass.UnitTests/RazorPagesTestHostTests.cs`.
- [X] T039 [P] [US3] Add regression test for viewing profile page after UI consolidation in `tests/Touching_Grass.UnitTests/ProfilePageRegressionTests.cs` (FR-017).
- [X] T040 [P] [US3] Add regression tests for editing preferred ZIP code, saving profile changes, and persistence behavior in `tests/Touching_Grass.UnitTests/ProfilePageRegressionTests.cs` (FR-017).
- [X] T041 [US3] Add workflow test confirming UI consolidation changes do not break profile-management navigation and profile save flow in `tests/Touching_Grass.UnitTests/ProfileWorkflowIntegrationTests.cs` (FR-017).

### Implementation for User Story 3

- [X] T026 [US3] Update shared layout nav labels and branding in `Touching_Grass/Pages/Shared/_Layout.cshtml`.
- [X] T027 [P] [US3] Standardize page titles in `Touching_Grass/Pages/Home.cshtml`, `Touching_Grass/Pages/Index.cshtml`, `Touching_Grass/Pages/Plants.cshtml`, `Touching_Grass/Pages/Results.cshtml`, `Touching_Grass/Pages/Privacy.cshtml`, `Touching_Grass/Pages/Account/Login.cshtml`, `Touching_Grass/Pages/Account/Register.cshtml`, and `Touching_Grass/Pages/Account/Profile.cshtml` using `Plantwise - <Page Purpose>` with unified landing title `Plantwise - Watering Guide`.
- [X] T028 [US3] Apply consistent visual treatment across `Touching_Grass/wwwroot/css/site.css`, `Touching_Grass/Pages/Index.cshtml`, `Touching_Grass/Pages/Home.cshtml`, `Touching_Grass/Pages/Plants.cshtml`, and `Touching_Grass/Pages/Results.cshtml`.
- [X] T029 [US3] Update Plant Search page messaging and presentation consistency in `Touching_Grass/Pages/Plants.cshtml`.

**Checkpoint**: All user stories are independently complete and cohesive.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final hardening, documentation, and validation.

- [X] T030 [P] Update user-facing copy and quick usage cues for concise messaging alignment in `Touching_Grass/Pages/Index.cshtml`, `Touching_Grass/Pages/Home.cshtml`, `Touching_Grass/Pages/Plants.cshtml`, and `Touching_Grass/Pages/Results.cshtml`.
- [X] T031 Validate keyboard accessibility and focus behavior for autocomplete and form controls in `Touching_Grass/Pages/Index.cshtml` and `Touching_Grass/wwwroot/js/site.js`.
- [X] T032 Validate responsive layout behavior for landing + suggestion UI in `Touching_Grass/wwwroot/css/site.css`.
- [X] T033 Run and fix all impacted automated tests in `tests/Touching_Grass.UnitTests/RazorPagesTestHostTests.cs`, `tests/Touching_Grass.UnitTests/IndexPageWorkflowTests.cs`, `tests/Touching_Grass.UnitTests/IndexPageAuthenticationTests.cs`, `tests/Touching_Grass.UnitTests/WateringCalculatorRegressionTests.cs`, `tests/Touching_Grass.UnitTests/ZipCodeValidationTests.cs`, `tests/Touching_Grass.UnitTests/NonCaliforniaZipWorkflowTests.cs`, `tests/Touching_Grass.UnitTests/NavigationAndTitleConsistencyTests.cs`, `tests/Touching_Grass.UnitTests/ProfilePageRegressionTests.cs`, and `tests/Touching_Grass.UnitTests/ProfileWorkflowIntegrationTests.cs`.
- [X] T034 Run full solution build and resolve any regressions.
- [X] T035 Execute quickstart verification steps from `specs/006-ui-ux-modernization/quickstart.md` and record completion notes in feature docs.
- [X] T046 [P] Create measurable usability checklist with pass/fail criteria for SC-005 in `specs/006-ui-ux-modernization/usability-validation-checklist.md`.
- [ ] T047 Execute timed discoverability validation for locating `Plant Search` within five seconds and record participant outcomes in `specs/006-ui-ux-modernization/usability-validation-checklist.md` (SC-005).
- [X] T048 Execute manual regression validation for profile workflows (view profile, edit preferred ZIP, save changes, and verify persisted ZIP reload) and record outcomes in `specs/006-ui-ux-modernization/quickstart.md` notes section (FR-017).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies; start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks all user-story work.
- **Phase 3 (US1)**: Depends on Phase 2; defines MVP.
- **Phase 4 (US2)**: Depends on Phase 2; can proceed in parallel with US1 after foundational completion if staffing allows.
- **Phase 5 (US3)**: Depends on Phase 2; can proceed in parallel with US1/US2 but should be validated against integrated UI.
- **Phase 6 (Polish)**: Depends on completion of all selected user stories.

### User Story Dependencies

- **US1 (P1)**: No dependency on other stories after foundational phase.
- **US2 (P2)**: No hard dependency on US1 logic, but must integrate with unified landing UI.
- **US3 (P3)**: Can be implemented independently after foundational tasks, then validated with US1/US2 pages.

### Within Each User Story

- Write tests first where practical, confirm failing expectation, then implement.
- Update page model/handlers before view/js wiring when behavior contracts change.
- Finish story-specific tests and validation before moving to the next priority.

### Parallel Opportunities

- Phase 1 tasks marked [P] can run in parallel.
- In Phase 2, T005 and T008 can run in parallel with non-overlapping files.
- In US2, T020 and T022 can proceed in parallel once handler contract is stable.
- In US3, title standardization (T027) can run parallel to style updates (T028).

---

## Parallel Example: User Story 2

```bash
# Parallel test authoring
Task: "T017 [US2] Add threshold/contains matching tests"
Task: "T018 [US2] Add max-10 and no-results tests"

# Parallel implementation on separate files
Task: "T020 [US2] Update autocomplete markup in Pages/Index.cshtml"
Task: "T021 [US2] Implement client autocomplete logic in wwwroot/js/site.js"
```

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1).
3. Validate unified landing workflow end-to-end.
4. Demo/deploy MVP behavior.

### Incremental Delivery

1. Foundation ready (Phases 1-2).
2. Deliver US1 and validate.
3. Deliver US2 and validate.
4. Deliver US3 and validate.
5. Execute polish and full regression checks.

### Parallel Team Strategy

1. Team completes setup/foundation together.
2. Split by story:
   - Developer A: US1 consolidation and in-page results.
   - Developer B: US2 autocomplete handler + JS.
   - Developer C: US3 titles/navigation/style standardization.
3. Rejoin for Phase 6 validation.

---

## Notes

- [P] tasks indicate low-conflict parallel work.
- Keep business logic, data schema, auth provider behavior, and weather integration unchanged.
- Ensure ZIP validation remains U.S.-wide and never reverts to California-only assumptions.

## Requirement & Success Criteria Traceability Matrix

| Key | Task IDs |
|-----|----------|
| FR-001 | T004, T012 |
| FR-002 | T010, T013 |
| FR-003 | T014, T015 |
| FR-004 | T011, T013 |
| FR-005 | T016, T038 |
| FR-006 | T016, T037 |
| FR-007 | T036, T037, T038 |
| FR-008 | T006, T017, T022 |
| FR-009 | T020, T022 |
| FR-010 | T007, T019, T022 |
| FR-011 | T018, T023 |
| FR-012 | T002, T028, T029 |
| FR-013 | T008, T024, T027 |
| FR-014 | T008, T024, T027 |
| FR-015 | T025, T026, T029 |
| FR-016 | T012, T030 |
| FR-017 | T039, T040, T041, T048 |
| FR-018 | T028, T032 |
| FR-019 | T020, T031 |
| FR-020 | T042, T043, T044, T045 |
| FR-021 | T006, T018, T022 |
| FR-022 | T024, T027 |
| SC-001 | T010, T013 |
| SC-002 | T016, T037 |
| SC-003 | T017, T018, T019 |
| SC-004 | T024, T027 |
| SC-005 | T046, T047 |
