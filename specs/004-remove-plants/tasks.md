# Tasks: Remove Plants from My Plants

**Input**: Design documents from `/specs/004-remove-plants/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Included because the project constitution requires automated tests for behavior changes.

**Organization**: Tasks are grouped by the two prioritized user stories. Story 2 builds on the removal flow from Story 1.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel with other marked tasks because it changes a different file and has no unmet dependencies.
- **[Story]**: User-story label for story-specific tasks.
- All implementation tasks name the repository file paths they change.

## Phase 1: Setup

**Purpose**: Confirm use of the existing application and test infrastructure; no project or dependency initialization is needed.

- [ ] T001 Verify the existing .NET 10 application, local/Azure service registrations, and xUnit test target in `Touching_Grass/Touching_Grass.csproj`, `Touching_Grass/Program.cs`, and `tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj`; do not add projects, packages, schema changes, or migrations.

## Phase 2: Foundational

**Purpose**: Blocking prerequisites shared by user stories.

No additional foundational work is required. Authentication, the shared service interface, both environment-specific implementations, and the test host already exist.

## Phase 3: User Story 1 - Remove a plant from My Plants (Priority: P1) 🎯 MVP

**Goal**: Remove only the authenticated user's selected saved-plant entry and confirm the removed plant by name, consistently in local and Azure environments.

**Independent Test**: Add a plant for a test user, remove it from My Plants, and verify it disappears from only that user's list, the catalog and another user's list remain unchanged, and the page confirms the plant by name.

### Tests for User Story 1

> Write these tests first and confirm they fail before implementation.

- [ ] T002 [P] [US1] Add `LocalUserPlantService.RemoveAsync` tests for successful removal, returned removed plant, missing entry, and preservation of another user's same catalog plant in `tests/Touching_Grass.UnitTests/LocalUserPlantServiceTests.cs`.
- [ ] T003 [P] [US1] Add `UserPlantService.RemoveAsync` tests using the existing EF Core InMemory provider for successful removal, missing entry, and owner isolation in `tests/Touching_Grass.UnitTests/UserPlantServiceTests.cs`.
- [ ] T004 [P] [US1] Add an authenticated My Plants POST integration test that checks removal and the plant-named success confirmation in `tests/Touching_Grass.UnitTests/MyPlantsPageTests.cs`.

### Implementation for User Story 1

- [ ] T005 [US1] Add `RemoveAsync` to `IUserPlantService` with the contract `Task<UserPlant?> RemoveAsync(string userId, int id, CancellationToken cancellationToken = default)` in `Touching_Grass/Services/IUserPlantService.cs`; `userId` is the required authenticated owner ID and `id` is the existing saved-entry ID.
- [ ] T006 [P] [US1] Implement local removal in `Touching_Grass/Services/LocalUserPlantService.cs` using the existing per-user lock, returning a clone of the removed entry or `null` when the required owner ID and saved-entry ID do not match an entry.
- [ ] T007 [P] [US1] Implement Azure-persisted removal in `Touching_Grass/Services/UserPlantService.cs`, matching both required `userId` and saved-entry `id`, deleting only that `UserPlant`, and returning the removed record or `null` without changing the schema.
- [ ] T008 [US1] Add an authenticated `OnPostRemoveAsync` handler in `Touching_Grass/Pages/MyPlants/Index.cshtml.cs` that derives the required owner ID from the current principal, passes only the submitted saved-entry ID to `RemoveAsync`, and sets a success message from the returned plant name before redirecting to My Plants.
- [ ] T009 [US1] Add a separate per-plant POST Remove form beside the existing favorite control and render the plant-named success message as a status in `Touching_Grass/Pages/MyPlants/Index.cshtml`; submit only the saved-entry ID and leave the home page and favorite behavior unchanged.

**Checkpoint**: A user can remove their own plant in either registered service environment, and successful removal produces a named confirmation.

## Phase 4: User Story 2 - Identify and safely use removal controls (Priority: P2)

**Goal**: Make each control identify its plant and provide accurate feedback when a stale or non-owned entry cannot be removed.

**Independent Test**: Verify each rendered Remove button's accessible name includes the plant name, then submit a stale or other-user entry ID and confirm that no other user's entry is removed and the page does not show success.

### Tests for User Story 2

> Write these tests first and confirm they fail before implementation.

- [ ] T010 [US2] Extend `tests/Touching_Grass.UnitTests/MyPlantsPageTests.cs` to assert the plant-specific accessible button name and non-success feedback for stale/non-owned IDs; inject a `DbUpdateException` and assert redirect to My Plants, the exact error message, no success message, and the entry remains saved.

### Implementation for User Story 2

- [ ] T011 [P] [US2] Change the visible button name in `Touching_Grass/Pages/MyPlants/Index.cshtml` to identify its action and plant (for example, `Remove Toyon`), and render failure feedback as an accessible message.
- [ ] T012 [P] [US2] Update `Touching_Grass/Pages/MyPlants/Index.cshtml.cs` so a `null` removal result sets generic non-success feedback without disclosing whether another user's entry exists; catch `DbException` and `DbUpdateException` from removal, log them, set `Something went wrong. Your plant was not removed.`, redirect to My Plants, and never show success for the failed operation. Allow cancellation to propagate.

**Checkpoint**: Removal controls identify their targets, and stale or non-owned requests receive accurate non-success feedback without affecting another user's list.

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validate the complete feature and its environment parity.

- [ ] T013 Run `dotnet test tests\Touching_Grass.UnitTests\Touching_Grass.UnitTests.csproj` and `dotnet build Touching_Grass.slnx`; resolve any failures without adding migrations or packages.
- [ ] T014 Complete the local and Azure scenarios in `specs/004-remove-plants/quickstart.md` with test accounts; verify ownership isolation, named feedback, and unchanged home-page/catalog behavior.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; confirms existing project infrastructure.
- **Foundational (Phase 2)**: Depends on Setup; no additional work is required.
- **User Story 1 (Phase 3)**: Starts after Setup; delivers the MVP removal flow.
- **User Story 2 (Phase 4)**: Depends on User Story 1 because its accessible control and failure feedback extend the removal form and handler.
- **Polish (Phase 5)**: Depends on both user stories.

### User Story Dependencies

- **User Story 1 (P1)**: No dependency on another feature story; requires only the existing application services and authenticated My Plants page.
- **User Story 2 (P2)**: Depends on User Story 1's removal operation and page form.

### Task Dependency Graph

```text
T001
 ├── T002 ─┐
 ├── T003 ─┼── T005 ──┬── T006 ─┐
 └── T004 ─┘           └── T007 ─┴── T008 ── T009 ── T010 ──┬── T011 ─┐
                                                             └── T012 ─┴── T013 ── T014
```

### Parallel Opportunities

- **User Story 1 tests**: T002, T003, and T004 work in separate test files and can be written in parallel.
- **Service implementations**: T006 and T007 can proceed in parallel after the shared interface contract in T005.
- **User Story 2 implementation**: T011 and T012 modify separate Razor files and can proceed in parallel after T010.
- **Final validation**: Complete T013 and then T014 after both stories; run the app validation after build/test completes.

## Parallel Example: User Story 1

```text
Task: T002 - Add local service removal tests in tests/Touching_Grass.UnitTests/LocalUserPlantServiceTests.cs
Task: T003 - Add Azure-persisted service removal tests in tests/Touching_Grass.UnitTests/UserPlantServiceTests.cs
Task: T004 - Add authenticated page removal test in tests/Touching_Grass.UnitTests/MyPlantsPageTests.cs
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001 and confirm no additional foundation is needed.
2. Write T002-T004 and verify they fail before implementation.
3. Implement T005-T009, then validate the P1 story independently.
4. Stop at the checkpoint if only the basic removal capability is needed.

### Incremental Delivery

1. Deliver User Story 1 with removal in both services and named success feedback.
2. Add User Story 2's accessible target name and stale/non-owned feedback.
3. Run automated tests/build and execute the local and Azure quickstart checks.

## Notes

- Test tasks are included to satisfy the project constitution's automated-testing requirement.
- `[P]` tasks operate on separate files and have no dependency on another incomplete task.
- No task changes the home page, `UserPlant` model, database schema, or migration files.
