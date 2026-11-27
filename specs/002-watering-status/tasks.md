# Tasks: Plain-Language Watering Status

**Input**: Design documents from `specs/002-watering-status/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `quickstart.md`

**Tests**: xUnit tests are explicitly requested. Write test tasks before the corresponding implementation tasks and verify behavior with the existing `tests/Touching_Grass.UnitTests` project. No dependencies are added.

**Organization**: Tasks are grouped by the prioritized user stories in `spec.md`. The existing .NET 10 web and xUnit projects are used.

## Phase 1: Setup

**Purpose**: The repository already has the .NET 10 Razor Pages application, solution, and xUnit test project. No project initialization or package changes are needed.

_No setup tasks._

---

## Phase 2: Foundational

**Purpose**: No shared infrastructure blocks this feature; existing service registration, Models/Services folders, and test project are ready.

_No foundational tasks._

---

## Phase 3: User Story 1 - Understand the weekly watering recommendation (Priority: P1) 🎯 MVP

**Goal**: Show the gardener the exact weekly status for zero, light, and standard watering needs, generated from the existing calculation without changing it.

**Independent Test**: Unit-test the exact status text using zero, light, 0.25 in, and larger needs; feed a result from `WateringCalculatorService` into the status service and confirm its message corresponds to `NetNeedInches`. Verify through `ResultsModel` that the status is available for a valid recommendation and no valid status is exposed for a negative input.

### Tests for User Story 1 (write before implementation)

- [X] T001 [P] [US1] Add xUnit cases in `tests/Touching_Grass.UnitTests/WaterStatusMessageServiceTests.cs` for zero (`0.00`), a need that rounds to zero (`0.004`), light watering (`0.14`), the `0.25` threshold, a larger need (`0.80`), and rejection of a negative need; assert the exact message strings and formatted amounts, and pass a result from `WateringCalculatorService` through the status service to confirm agreement with `NetNeedInches` without changing the calculator.
- [X] T002 [P] [US1] Add xUnit cases in `tests/Touching_Grass.UnitTests/ResultsModelTests.cs` proving that a valid deserialized recommendation's `NetNeedInches` is passed to the injected status service and its returned `WaterStatusMessage` is exposed to the page, and that negative input produces no valid status.

### Implementation for User Story 1

- [X] T003 [US1] Add `WaterStatusMessage` with a `Message` string property in `Touching_Grass/Models/WaterStatusMessage.cs`.
- [X] T004 [US1] Add `IWaterStatusMessageService.Create(decimal netNeedInches)` returning `WaterStatusMessage` in `Touching_Grass/Services/IWaterStatusMessageService.cs`.
- [X] T005 [US1] Implement `WaterStatusMessageService` in `Touching_Grass/Services/WaterStatusMessageService.cs` to reject negative inputs, round to two decimals using midpoint rounding away from zero before classification, and return the exact zero/light/standard messages using the rounded value.
- [X] T006 [US1] Register `IWaterStatusMessageService` to `WaterStatusMessageService` as a singleton in `Touching_Grass/Program.cs` without adding packages or changing existing registrations.
- [X] T007 [US1] Inject `IWaterStatusMessageService` into `ResultsModel` in `Touching_Grass/Pages/Results.cshtml.cs`, create `WaterStatusMessage` from `Recommendation.NetNeedInches`, and ensure an `ArgumentOutOfRangeException` from invalid negative input leaves no valid recommendation status and uses the existing unavailable-recommendation feedback pattern.
- [X] T008 [US1] Render `Model.WaterStatusMessage.Message` with the existing recommendation in `Touching_Grass/Pages/Results.cshtml`; keep rounding, classification, and message construction out of the Razor page.

**Checkpoint**: A valid Results recommendation has an exact plain-language status; existing `WateringCalculatorService` calculations remain unchanged.

---

## Phase 4: User Story 2 - Trust the displayed amount and wording (Priority: P2)

**Goal**: Ensure rounding, category selection, and displayed numeric formatting always agree, including midpoint and threshold-adjacent values.

**Independent Test**: Verify `0.005` becomes `0.01` and is light watering, `0.244` becomes `0.24` and is light watering, and `0.245` becomes `0.25` and is standard watering. Confirm nonzero formatting remains two decimals with a period and `in` under a culture that uses a decimal comma.

### Tests for User Story 2 (write before implementation refinements)

- [X] T009 [US2] Extend `tests/Touching_Grass.UnitTests/WaterStatusMessageServiceTests.cs` with away-from-zero midpoint cases (`0.005`, `0.245`), the threshold-adjacent case (`0.244`), and a non-English current-culture case that asserts invariant `X.XX in` output.

### Implementation for User Story 2

- [X] T010 [US2] Update `Touching_Grass/Services/WaterStatusMessageService.cs` as needed so both the category and message amount derive from the same rounded decimal and nonzero amounts use invariant-culture `0.00` formatting followed by `in`; preserve exact specified wording.

**Checkpoint**: Every tested amount and its status category agree after midpoint rounding, regardless of the machine's current decimal separator.

---

## Phase 5: User Story 3 - Receive an accessible status update (Priority: P3)

**Goal**: Make the Results status discoverable as an accessible status update.

**Independent Test**: Open a valid Results page, inspect the rendered message and accessibility tree, and verify the message is inside an element with `role="status"` and is available to assistive technology.

### Implementation for User Story 3

- [X] T011 [US3] Render the watering status only when valid in a `role="status"` region in `Touching_Grass/Pages/Results.cshtml` and verify the rendered page during the manual accessibility check.
- [X] T012 [P] [US3] Update `specs/002-watering-status/quickstart.md` with the manual accessibility check for the Results message's `role="status"` semantics and agreement with the recommendation.

**Checkpoint**: The final Results page exposes only its watering message as a status update and preserves the existing recommendation presentation.

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: Run all feature validation and check that existing calculation behavior remains unchanged.

- [X] T013 Run `dotnet test tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj` and `dotnet build Touching_Grass/Touching_Grass.csproj`; resolve failures caused by this feature.
- [X] T014 Complete the end-to-end and accessibility scenarios in `specs/002-watering-status/quickstart.md`, including confirming that the existing `WateringCalculatorService` output is unchanged.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No work required; existing .NET 10 projects are in place.
- **Foundational (Phase 2)**: No work required; existing project structure and DI setup are sufficient.
- **User Story 1 (Phase 3)**: Starts immediately; provides the first complete recommendation status.
- **User Story 2 (Phase 4)**: Depends on the `WaterStatusMessageService` from US1; adds edge-case and invariant-format assurance.
- **User Story 3 (Phase 5)**: Depends on the Results integration from US1; adds accessible status semantics. US2 and US3 otherwise have no dependency on one another and can be implemented in parallel after US1.
- **Polish**: Depends on all three user stories.

### User Story Dependencies

- **US1 (P1)**: Independent; no feature dependencies.
- **US2 (P2)**: Uses the US1 status service; no Results markup dependency.
- **US3 (P3)**: Uses the US1 Results message; does not depend on US2's boundary refinements.

### Parallel Opportunities

- **US1 test writing**: T001 and T002 can be authored in parallel because they modify different test files; both precede implementation.
- **US2 and US3**: After US1 is complete, their service-formatting and accessibility work can proceed in parallel, subject to avoiding simultaneous edits to shared files.
- **Documentation**: T012 can be performed independently of source changes because it edits only the quickstart guide.

### Parallel Example: User Story 1

```text
Task: T001 - Add service behavior tests in tests/Touching_Grass.UnitTests/WaterStatusMessageServiceTests.cs
Task: T002 - Add ResultsModel tests in tests/Touching_Grass.UnitTests/ResultsModelTests.cs
```

### Parallel Example: User Stories 2 and 3

```text
After User Story 1 is complete:
Task: T009/T010 - Add rounding and culture tests, then refine Touching_Grass/Services/WaterStatusMessageService.cs
Task: T011 - Add the role="status" region in Touching_Grass/Pages/Results.cshtml
Task: T012 - Update specs/002-watering-status/quickstart.md
```

## Implementation Strategy

### MVP First (User Story 1)

1. Write T001 and T002 before feature implementation.
2. Implement the model, interface, service, DI registration, ResultsModel handoff, and visible status (T003-T008).
3. Run focused xUnit tests to validate the P1 status behavior.
4. Complete US2 rounding/culture assurance and US3 accessibility semantics before considering the full feature complete.

### Incremental Delivery

1. Complete US1 and verify the main recommendation categories without changing the existing calculation.
2. Complete US2 and verify midpoint, threshold-adjacent, invalid-input, and invariant-formatting behavior.
3. Complete US3 and verify status-region semantics in the rendered Results page.
4. Run all validation commands in the final phase.

## Notes

- Every task uses the required checkbox, sequential ID, optional parallel marker, user-story label in story phases, and an exact project-relative file path.
- `WateringCalculatorService` is a test input/source of a recommendation only; do not modify its implementation or calculation.
- The user-facing text is fixed English copy; do not introduce localization, per-plant thresholds, or weather-based wording.
- Commit behavior is not included in this task list.
