# Tasks: Azure SQL Managed Identity Persistence

**Input**: Design documents from `/specs/005-azure-sql-managed-identity/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/deployment-contract.md, quickstart.md

**Tests**: Automated tests are required by constitution and by feature success criteria.

**Organization**: Tasks are grouped by user story and include explicit implementation + validation coverage.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency)
- **[Story]**: User story mapping (`US1`, `US2`, `US3`) or `FOUNDATION`
- Use exact file paths in every task

## Canonical Production Configuration Contract

- **Authoritative key**: `ConnectionStrings:DefaultConnection`
- **Azure source**: App Service Connection String entry named `DefaultConnection`
- No alternate key names are allowed in implementation tasks

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare dependencies and design artifacts for Azure SQL + managed identity transition.

- [ ] T001 [FOUNDATION] Update persistence packages in `Touching_Grass/Touching_Grass.csproj` (remove MySQL provider, add/confirm SQL Server provider + managed-identity-compatible SQL client dependencies).
- [ ] T002 [P] [FOUNDATION] Update feature docs in `specs/005-azure-sql-managed-identity/quickstart.md` with canonical key `ConnectionStrings:DefaultConnection` and App Service connection-string source contract.
- [ ] T003 [P] [FOUNDATION] Update `specs/005-azure-sql-managed-identity/contracts/deployment-contract.md` with explicit least-privilege + no-`db_owner` validation contract.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Complete all startup, migration, and permission foundations before user-story implementation.

**CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T004 [FOUNDATION] Implement non-Development provider selection in `Touching_Grass/Program.cs` using only `ConnectionStrings:DefaultConnection` and fail startup when key is missing/invalid (FR-015, FR-016).
- [ ] T005 [FOUNDATION] Implement the **single authoritative managed-identity database configuration** in `Touching_Grass/Program.cs` (token-based Azure SQL auth path; no SQL username/password runtime dependency) (FR-003, FR-004, FR-005).
- [ ] T006 [FOUNDATION] Implement non-Development migration/readiness gate behavior in `Touching_Grass/Program.cs` so readiness is blocked until connectivity + migrations succeed (FR-008, FR-010, FR-016).
- [ ] T007 [P] [FOUNDATION] Regenerate active SQL Server migration artifacts in `Touching_Grass/Data/Migrations/` and snapshot for current model (FR-002, FR-009).
- [ ] T008 [P] [FOUNDATION] Archive MySQL runtime migration artifacts and update include/exclude rules in `Touching_Grass/Touching_Grass.csproj` (FR-011).
- [ ] T009 [FOUNDATION] Update startup/migration validation tests in `tests/Touching_Grass.UnitTests/MigrationValidationTests.cs` for canonical key and SQL Server migration path (FR-008, FR-009, FR-015).
- [ ] T010 [FOUNDATION] Update composition boundary tests in `tests/Touching_Grass.UnitTests/CompositionBoundaryTests.cs` for environment split, canonical key enforcement, and fail-safe startup behavior (FR-001, FR-013, FR-016).

### Azure SQL Managed Identity Permission Implementation & Validation (Required)

- [ ] T011 [FOUNDATION] Add managed identity SQL user provisioning script and role grants in `scripts/sql/005-create-mi-user-least-privilege.sql` (create MI user, grant only approved least-privilege roles needed for runtime + migrations) (FR-014).
- [ ] T012 [FOUNDATION] Add permission verification script in `scripts/sql/005-verify-mi-role-membership.sql` to assert required roles are present and `db_owner` is absent (FR-018, SC-005).
- [ ] T013 [FOUNDATION] Add deployment runbook steps in `specs/005-azure-sql-managed-identity/quickstart.md` to execute scripts, capture outputs, and store evidence for each validation run (FR-017, FR-018, SC-005).

### Startup Readiness Measurement (Required)

- [ ] T014 [FOUNDATION] Add startup-readiness measurement script in `scripts/measure-startup-readiness.ps1` that measures from deployment completion to first healthy readiness response and writes timestamped evidence output (SC-003).
- [ ] T015 [FOUNDATION] Add readiness pass/fail rule to `specs/005-azure-sql-managed-identity/contracts/deployment-contract.md`: pass only if readiness <= 5 minutes after deployment; otherwise fail release validation (SC-003).

**Checkpoint**: Foundation ready; user stories can proceed.

---

## Phase 3: User Story 1 - Keep account and saved data persistent in cloud (Priority: P1) ?? MVP

**Goal**: Ensure Azure runtime persists Identity and user-owned data in Azure SQL, including empty-database initialization.

**Independent Test**: Deploy in non-Development mode, initialize empty DB via migration, register/login user, save/retrieve plant data, restart app, and verify persistence.

### Tests for User Story 1

- [ ] T016 [P] [US1] Update `tests/Touching_Grass.UnitTests/WorkflowRegressionIntegrationTests.cs` with Azure persistence flow checks for register/login/save/retrieve across restart (FR-006, FR-007, SC-002).
- [ ] T017 [P] [US1] Update `tests/Touching_Grass.UnitTests/UserPlantServiceTests.cs` for SQL-backed user-owned persistence and ownership isolation assertions (FR-007).

### Implementation for User Story 1

- [ ] T018 [US1] Update SQL-provider model constraints in `Touching_Grass/Data/ApplicationDbContext.cs` to remain Azure SQL compatible (FR-002, FR-007).
- [ ] T019 [US1] Update SQL-backed persistence behavior in `Touching_Grass/Services/UserPlantService.cs` for provider-consistent operations and ownership filtering (FR-007).
- [ ] T020 [US1] Update authenticated persistence flows in `Touching_Grass/Pages/Account/Profile.cshtml.cs` and `Touching_Grass/Pages/MyPlants/Index.cshtml.cs` to preserve existing behavior against Azure SQL storage (FR-006, FR-007).

**Checkpoint**: US1 independently functional.

---

## Phase 4: User Story 2 - Keep local development workflow unchanged (Priority: P2)

**Goal**: Preserve existing Development-mode in-memory behavior with no Azure resource requirement.

**Independent Test**: Run locally with no Azure SQL configuration and verify startup + local flows remain functional.

### Tests for User Story 2

- [ ] T021 [P] [US2] Update `tests/Touching_Grass.UnitTests/DevelopmentIdentityTests.cs` for Development-mode in-memory behavior parity (FR-001, FR-012).
- [ ] T022 [P] [US2] Update test-host environment coverage in `tests/Touching_Grass.UnitTests/RazorPagesTestHost.cs` and `tests/Touching_Grass.UnitTests/TestHostEnvironment.cs` to ensure no Azure SQL dependency in Development (FR-001, FR-012).

### Implementation for User Story 2

- [ ] T023 [US2] Update developer documentation in `README.md` and `specs/005-azure-sql-managed-identity/quickstart.md` to confirm unchanged local workflow and canonical production key usage (FR-012, FR-015, SC-001).

**Checkpoint**: US2 independently functional.

---

## Phase 5: User Story 3 - Use passwordless cloud access for the app identity (Priority: P3)

**Goal**: Validate passwordless managed identity operation using least privilege only.

**Independent Test**: Run non-Development startup with managed identity only, verify migrations/readiness and normal operations, prove no `db_owner` requirement.

### Tests for User Story 3

- [ ] T024 [P] [US3] Add startup-failure tests in `tests/Touching_Grass.UnitTests/MigrationValidationTests.cs` for missing/invalid `ConnectionStrings:DefaultConnection` and connectivity failures (FR-015, FR-016, SC-006).
- [ ] T025 [P] [US3] Add readiness/health behavior tests in `tests/Touching_Grass.UnitTests/CompositionBoundaryTests.cs` for blocked readiness when DB unavailable (FR-010, FR-016, SC-006).

### Implementation & Validation for User Story 3

- [ ] T026 [US3] Update deployment workflow in `.github/workflows/master_plantwisewatertracking.yml` to use Azure SQL validation contract and collect readiness timing + permission evidence artifacts (SC-003, SC-005).
- [ ] T027 [US3] Execute least-privilege validation runbook and record evidence in `specs/005-azure-sql-managed-identity/quickstart.md`: prove startup migration + app operations succeed with least privilege only (FR-017, SC-005).
- [ ] T028 [US3] Execute negative validation and evidence capture in `specs/005-azure-sql-managed-identity/quickstart.md`: confirm final role membership excludes `db_owner` and other broad admin roles (FR-018, SC-005).

**Checkpoint**: US3 independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final consistency and traceability closure.

- [ ] T029 [P] Update `.github/copilot-instructions.md` to reflect Azure SQL + canonical key architecture as a governance-maintainability update (non-blocking documentation task).
- [ ] T030 Run full regression tests and summarize results in `specs/005-azure-sql-managed-identity/quickstart.md`.
- [ ] T031 Remove obsolete MySQL references from `Touching_Grass/Program.cs`, `Touching_Grass/Touching_Grass.csproj`, `README.md`, and `.github/workflows/master_plantwisewatertracking.yml`.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phases 3–5 (User Stories)**: Depend on Phase 2 completion.
- **Phase 6 (Polish)**: Depends on completion of desired user stories.

### Program.cs Sequencing Rule (Conflict Prevention)

- Tasks `T004 -> T005 -> T006` are strictly sequential and modify `Touching_Grass/Program.cs`.
- No other phase may introduce parallel `Program.cs` edits.
- User story phases must consume the foundation behavior rather than re-implement startup configuration.

### Parallel Opportunities

- Setup docs tasks `T002`, `T003` can run in parallel.
- Migration/archive tasks `T007`, `T008` can run in parallel.
- Per-story test tasks marked `[P]` can run in parallel.
- Do not parallelize tasks that modify the same file.

---

## Requirement & Success Criteria Coverage Matrix

| Requirement | Implementation Tasks | Validation Tasks |
|-------------|----------------------|------------------|
| FR-001 | T004 | T010, T021, T022 |
| FR-002 | T004, T007, T018 | T009 |
| FR-003 | T005 | T026, T027 |
| FR-004 | T005 | T027 |
| FR-005 | T005 | T027 |
| FR-006 | T020 | T016 |
| FR-007 | T019, T020 | T016, T017 |
| FR-008 | T006, T007 | T009, T027 |
| FR-009 | T007 | T009, T016 |
| FR-010 | T006 | T025 |
| FR-011 | T001, T008, T031 | T030 |
| FR-012 | T023 | T021, T022 |
| FR-013 | T004 | T010 |
| FR-014 | T011 | T012, T027 |
| FR-015 | T004 | T009, T024 |
| FR-016 | T006 | T024, T025 |
| FR-017 | T013 | T027 |
| FR-018 | T012, T013 | T028 |
| SC-001 | T023 | T021, T022 |
| SC-002 | T019, T020 | T016, T017 |
| SC-003 | T014, T015 | T026 |
| SC-004 | T005 | T027 |
| SC-005 | T011, T012, T013 | T027, T028 |
| SC-006 | T006 | T024, T025 |

---

## Implementation Strategy

### MVP First (US1)

1. Complete Phases 1–2.
2. Deliver US1 (Phase 3) and validate persistence independently.
3. Confirm migrations and restart persistence on Azure SQL.

### Incremental Delivery

1. Foundation complete (Phases 1–2).
2. US1 for cloud persistence.
3. US2 for local workflow parity.
4. US3 for managed identity least-privilege validation.
5. Phase 6 polish and documentation cleanup.
