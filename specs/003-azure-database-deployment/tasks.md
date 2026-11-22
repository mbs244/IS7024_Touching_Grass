# Tasks: Azure Persistent User Data

**Feature**: Azure Persistent User Data
**Specification**: [spec.md](./spec.md)
**Plan**: [plan.md](./plan.md)
**Architecture decision**: Azure MySQL In-App; EF Core migrations execute during application startup; GitHub Actions validates migrations but does not apply production migrations; one App Service instance and one production environment.

## Phase 1: MySQL Provider Migration

**Purpose**: Replace SQL Server-specific persistence with a validated MySQL-compatible EF Core configuration and migration model.

### T001 - Confirm the MySQL provider and Azure MySQL In-App connection contract

- **Files affected**: `Touching_Grass/Touching_Grass.csproj`, `Touching_Grass/Program.cs`, `specs/003-azure-database-deployment/research.md`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: None
- **Traceability**: [FR-016] [FR-017] [FR-026] [FR-029] [NFR-007] [NFR-008] [NFR-009]
- **Acceptance criteria**:
  - The selected MySQL EF Core provider and version are documented.
  - Provider compatibility with .NET 10 and EF Core 10 is verified.
  - The Azure MySQL In-App connection-string format and logical `DefaultConnection` configuration name are documented.
  - No production secret is added to the repository.

### T002 - Replace SQL Server EF Core dependencies with the validated MySQL provider

- **Files affected**: `Touching_Grass/Touching_Grass.csproj`, `tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj`
- **Dependencies**: T001
- **Traceability**: [FR-017] [FR-020] [NFR-006] [NFR-007]
- **Acceptance criteria**:
  - The application references the selected MySQL-compatible EF Core provider.
  - SQL Server is no longer the runtime provider for the target deployment.
  - Package restore completes without an incompatible provider or EF Core version conflict.
  - The test project remains compatible with the application project.

### T003 - Replace SQL Server runtime registration with MySQL registration

- **Files affected**: `Touching_Grass/Program.cs`
- **Dependencies**: T001, T002
- **Traceability**: [FR-016] [FR-017] [FR-021] [FR-023] [FR-028] [NFR-003] [NFR-005]
- **Acceptance criteria**:
  - Runtime database registration uses the selected MySQL provider.
  - The application resolves the production database through the approved `DefaultConnection` contract.
  - LocalDB is not selected when the application runs in Azure.
  - Missing production configuration produces a safe, explicit failure.
  - No connection-string value is logged or exposed in an exception response.

### T004 - Isolate local and production database configuration

- **Files affected**: `Touching_Grass/appsettings.json`, `Touching_Grass/appsettings.Development.json`, `.gitignore`, `README.md`
- **Dependencies**: T001, T003
- **Traceability**: [FR-025] [FR-026] [FR-027] [FR-028] [FR-029] [NFR-005]
- **Acceptance criteria**:
  - Local development uses a non-production MySQL-compatible connection source.
  - Production credentials are not stored in tracked JSON files.
  - The local database is separate from the Azure database.
  - Documentation explains how protected Azure configuration supplies `DefaultConnection`.

### T005 - Regenerate the EF Core migration set for MySQL

- **Files affected**: `Touching_Grass/Data/Migrations/`, `Touching_Grass/Data/ApplicationDbContext.cs`, `Touching_Grass/Data/Migrations.SqlServerArchive/[NEW]`
- **Dependencies**: T002, T003, T004
- **Traceability**: [FR-018] [FR-019] [FR-020] [FR-022] [AC-008] [NFR-006]
- **Acceptance criteria**:
  - The migration model is generated using the validated MySQL provider.
  - SQL Server-specific migration operations are not used as the MySQL production migration set.
  - Identity tables, `ApplicationUser` profile data, `UserPlant`, ownership relationships, indexes, uniqueness constraints, and migration history are represented.
  - The migration applies successfully to a clean isolated MySQL-compatible database.
  - Rerunning the migration is safe and does not erase existing data.
  - Existing SQL Server migration files are preserved in Git history but are archived outside the active migration directory; only MySQL migrations are discoverable at runtime.

### T006 - Validate the MySQL migration model and provider build

- **Files affected**: `tests/Touching_Grass.UnitTests/MigrationValidationTests.cs [NEW]`, `Touching_Grass/Data/Migrations/`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T005
- **Traceability**: [FR-017] [FR-020] [FR-022] [AC-008] [SC-005] [NFR-006]
- **Acceptance criteria**:
  - The application and test projects restore and build successfully.
  - Migration validation identifies the expected schema without SQL Server provider errors.
  - A clean database migration test passes.
  - A repeated migration test passes without duplicate objects or destructive changes.
  - `tests/Touching_Grass.UnitTests/MigrationValidationTests.cs` is the planned focused validation fixture; the directory-level migration reference is not used here.

## Phase 2: Identity Persistence

**Purpose**: Validate registration, login, logout, password rules, and persistence through ASP.NET Core Identity using the MySQL database.

### T007 - Validate the ApplicationUser Identity model for MySQL

- **Files affected**: `Touching_Grass/Models/ApplicationUser.cs`, `Touching_Grass/Data/ApplicationDbContext.cs`, `Touching_Grass/Data/Migrations/`
- **Dependencies**: T005
- **Traceability**: [FR-001] [FR-003] [FR-018] [FR-020] [AC-001] [NFR-007]
- **Acceptance criteria**:
  - The user identifier, normalized email, security fields, and default ZIP profile data map correctly to MySQL.
  - Required and maximum-length constraints are preserved.
  - Normalized email uniqueness is enforced by the database and Identity configuration.
  - No email-verification or password-reset dependency is introduced.

### T007A - Document and resolve the existing Identity UI approach

- **Files affected**: `Touching_Grass/Pages/Account/Register.cshtml`, `Touching_Grass/Pages/Account/Register.cshtml.cs`, `Touching_Grass/Pages/Account/Login.cshtml`, `Touching_Grass/Pages/Account/Login.cshtml.cs`, `Touching_Grass/Pages/Account/Logout.cshtml`, `Touching_Grass/Pages/Account/Logout.cshtml.cs`, `Touching_Grass/Pages/Account/Profile.cshtml`, `Touching_Grass/Pages/Account/Profile.cshtml.cs`, `Touching_Grass/Areas/Identity/Pages/[NEW only if required]`
- **Dependencies**: T007
- **Traceability**: [FR-001] [FR-004] [FR-005] [FR-040] [NFR-007]
- **Acceptance criteria**:
  - The implementation records that the repository uses custom Razor Pages under `Touching_Grass/Pages/Account`, not scaffolded `Areas/Identity/Pages` or MVC controllers.
  - Existing registration, login, logout, and profile files are reused where suitable; new Identity files are marked `[NEW]` before creation.
  - Subsequent Identity tasks use these exact paths and do not invent an `Areas/Identity` location.

### T008 - Validate user registration and password rules

- **Files affected**: `Touching_Grass/Program.cs`, `Touching_Grass/Pages/Account/Register.cshtml`, `Touching_Grass/Pages/Account/Register.cshtml.cs`, `tests/Touching_Grass.UnitTests/RegistrationTests.cs`
- **Dependencies**: T006, T007, T007A
- **Traceability**: [FR-001] [FR-002] [FR-003] [FR-006] [FR-007] [FR-008] [AC-001] [AC-004] [NFR-005]
- **Acceptance criteria**:
  - A valid unique email and compliant password create exactly one account.
  - Duplicate normalized email registration is rejected.
  - Missing, malformed, and duplicate email inputs are rejected safely.
  - Password rules are enforced server-side, including requests that bypass client validation.
  - Passwords and password hashes are not displayed or logged.

### T009 - Validate login and authenticated session behavior

- **Files affected**: `Touching_Grass/Program.cs`, `Touching_Grass/Pages/Account/Login.cshtml`, `Touching_Grass/Pages/Account/Login.cshtml.cs`, `tests/Touching_Grass.UnitTests/LoginTests.cs`
- **Dependencies**: T008, T007A
- **Traceability**: [FR-004] [FR-006] [FR-007] [FR-040] [AC-002] [NFR-002] [NFR-005]
- **Acceptance criteria**:
  - Valid credentials authenticate against MySQL-backed Identity records.
  - Invalid credentials do not authenticate.
  - Authentication failure responses do not disclose whether the email or password was incorrect.
  - Authenticated users can access protected pages.
  - Authentication cookies use secure production behavior.

### T010 - Validate logout and protected-page denial

- **Files affected**: `Touching_Grass/Pages/Account/Logout.cshtml`, `Touching_Grass/Pages/Account/Logout.cshtml.cs`, `Touching_Grass/Pages/Account/Profile.cshtml`, `Touching_Grass/Pages/Account/Profile.cshtml.cs`, `tests/Touching_Grass.UnitTests/LogoutTests.cs`
- **Dependencies**: T009, T007A
- **Traceability**: [FR-005] [FR-010] [FR-041] [AC-003] [NFR-005]
- **Acceptance criteria**:
  - Logout ends the authenticated session.
  - The next request to a protected page is redirected or denied.
  - No protected user data is returned after logout.
  - Logout behavior is covered by an automated test.

### T011 - Validate Identity persistence across application restart

- **Files affected**: `tests/Touching_Grass.UnitTests/IdentityPersistenceTests.cs`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T008, T009, T010
- **Traceability**: [FR-018] [FR-024] [AC-006] [SC-003] [NFR-001] [NFR-004]
- **Acceptance criteria**:
  - An account created before restart remains available afterward.
  - The user can log in after restart without re-registration.
  - Identity records are stored in MySQL rather than process memory or JSON.
  - The test uses an isolated non-production database.

## Phase 3: User Data Persistence

**Purpose**: Persist approved MVP profile and plant/favorite data while enforcing ownership for every operation.

### T012 - Validate the user-owned data model and migration constraints

- **Files affected**: `Touching_Grass/Models/UserPlant.cs`, `Touching_Grass/Data/ApplicationDbContext.cs`, `Touching_Grass/Data/Migrations/`, `specs/003-azure-database-deployment/data-model.md`
- **Dependencies**: T005, T007
- **Traceability**: [FR-009] [FR-011] [FR-013] [FR-018] [FR-019] [AC-005] [NFR-006]
- **Acceptance criteria**:
  - `UserPlant` has a required relationship to exactly one `ApplicationUser`.
  - User ID, source plant ID, name snapshots, favorite state, and audit timestamps are persisted.
  - Required max lengths and nullability constraints from the data model are enforced.
  - The user/source-plant uniqueness rule is enforced.
  - Delete behavior does not leave unauthorized orphaned records.

### T013 - Validate profile and default ZIP persistence

- **Files affected**: `Touching_Grass/Pages/Account/Profile.cshtml`, `Touching_Grass/Pages/Account/Profile.cshtml.cs`, `Touching_Grass/Models/ApplicationUser.cs`, `Touching_Grass/Services/ZipLookupService.cs`, `tests/Touching_Grass.UnitTests/ProfilePersistenceTests.cs`
- **Dependencies**: T012
- **Traceability**: [FR-009] [FR-010] [FR-011] [FR-015] [FR-040] [AC-005] [NFR-001] [NFR-003]
- **Acceptance criteria**:
  - A valid five-digit United States ZIP selection is stored for the authenticated user.
  - Invalid ZIP values are rejected before persistence.
  - A user can retrieve the same default ZIP after logout and login.
  - One user cannot read or change another user's profile ZIP.

### T014 - Validate saved plant and favorite persistence

- **Files affected**: `Touching_Grass/Repositories/PlantRepository.cs`, `Touching_Grass/Services/UserPlantService.cs`, `Touching_Grass/Pages/Index.cshtml`, `Touching_Grass/Pages/Index.cshtml.cs`, `Touching_Grass/Pages/MyPlants/Index.cshtml`, `Touching_Grass/Pages/MyPlants/Index.cshtml.cs`, `Touching_Grass/Pages/MyPlants/Details.cshtml`, `Touching_Grass/Pages/MyPlants/Details.cshtml.cs`, `tests/Touching_Grass.UnitTests/UserPlantPersistenceTests.cs`
- **Dependencies**: T012, T013
- **Traceability**: [FR-009] [FR-014] [FR-018] [FR-024] [AC-005] [AC-006] [NFR-001] [NFR-004]
- **Acceptance criteria**:
  - An authenticated user can save an approved plant record.
  - Favorite state and audit timestamps are persisted.
  - A duplicate source plant for the same user follows the defined uniqueness behavior.
  - Saved records remain available after logout/login and application restart.
  - Existing read-only JSON plant data remains unchanged unless explicitly required by the feature.

### T015 - Enforce server-side user ownership for all user-data operations

- **Files affected**: `Touching_Grass/Services/UserPlantService.cs`, `Touching_Grass/Pages/Index.cshtml.cs`, `Touching_Grass/Pages/MyPlants/Index.cshtml.cs`, `Touching_Grass/Pages/MyPlants/Details.cshtml.cs`, `Touching_Grass/Pages/Account/Profile.cshtml.cs`, `tests/Touching_Grass.UnitTests/UserOwnershipTests.cs`
- **Dependencies**: T012, T014
- **Traceability**: [FR-010] [FR-011] [FR-012] [FR-013] [AC-005] [AC-011] [NFR-003] [NFR-005]
- **Acceptance criteria**:
  - Create operations derive the owner from the authenticated principal.
  - Read, update, and delete queries filter by the authenticated user ID.
  - Client-supplied user IDs cannot change ownership.
  - Cross-user record access fails without revealing whether the record exists.
  - Tests cover missing authentication, direct identifier manipulation, and two-user isolation.

### T016 - Validate user-data persistence after restart and redeployment

- **Files affected**: `tests/Touching_Grass.UnitTests/UserDataPersistenceTests.cs`, `specs/003-azure-database-deployment/quickstart.md`, `specs/003-azure-database-deployment/contracts/deployment-contract.md`
- **Dependencies**: T014, T015
- **Traceability**: [FR-014] [FR-024] [AC-006] [AC-007] [SC-003] [SC-004] [NFR-001] [NFR-004]
- **Acceptance criteria**:
  - Data saved before an application restart can be retrieved afterward.
  - Data saved before a later application deployment remains retrievable afterward.
  - No reseeding, re-registration, or JSON fallback is required.
  - The verification records the application artifact and database migration version used.

## Phase 4: Migration Validation and Deployment Gating

**Purpose**: Make GitHub Actions validate provider compatibility, tests, migration artifacts, and packaging without applying production migrations.

### T017 - Add the complete test execution stage to GitHub Actions

- **Files affected**: `.github/workflows/master_plantwisewatertracking.yml`
- **Dependencies**: T006, T008, T009, T010, T015
- **Traceability**: [FR-031] [FR-032] [FR-033] [FR-034] [SC-006] [NFR-009]
- **Acceptance criteria**:
  - The workflow runs the complete unit and required persistence/ownership test suite.
  - Test execution occurs after restore/build and before publish/deployment.
  - A failing test prevents the deployment job from running.
  - Test output does not expose secrets or personal data.

### T018 - Add CI migration validation without production migration application

- **Files affected**: `.github/workflows/master_plantwisewatertracking.yml`, `Touching_Grass/Data/Migrations/`, `specs/003-azure-database-deployment/contracts/deployment-contract.md`
- **Dependencies**: T006, T017
- **Traceability**: [FR-020] [FR-034] [FR-037] [AC-008] [SC-005] [SC-007] [NFR-006] [NFR-009]
- **Acceptance criteria**:
  - CI validates that the MySQL migration artifacts can be discovered and generated/applied against an isolated validation database when available.
  - The workflow does not apply migrations to the production MySQL In-App database.
  - Migration validation failure prevents publishing or deployment.
  - The workflow documents that production migration execution occurs during application startup.
  - `Touching_Grass/Data/Migrations/` is intentionally retained as a directory-level input because validation examines the active migration classes and generated model snapshot as a set; this is not a direction to hand-edit all generated artifacts.
  - Generated migration artifacts are reviewed and regenerated through EF Core tooling rather than casually hand-edited.

### T019 - Gate publishing and deployment on validation success

- **Files affected**: `.github/workflows/master_plantwisewatertracking.yml`
- **Dependencies**: T017, T018
- **Traceability**: [FR-034] [FR-035] [FR-036] [FR-039] [SC-006] [SC-007] [NFR-005] [NFR-009]
- **Acceptance criteria**:
  - The publish step runs only after restore, build, tests, and migration validation succeed.
  - The deployment job depends on the validated build/publish job.
  - The workflow continues to target `PlantwiseWaterTracking` and the intended production slot.
  - The workflow does not contain production connection strings or passwords.

### T020 - Add startup-migration failure and readiness verification

- **Files affected**: `Touching_Grass/Program.cs`, `tests/Touching_Grass.UnitTests/StartupMigrationTests.cs [NEW]`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T003, T005, T018
- **Traceability**: [FR-021] [FR-022] [FR-023] [FR-028] [FR-037] [FR-038] [AC-008] [AC-012] [SC-010] [SC-011] [NFR-001] [NFR-003] [NFR-009]
- **Acceptance criteria**:
  - Application startup applies pending EF Core migrations before serving normal database-dependent traffic.
  - Startup does not use database creation shortcuts that bypass migration history.
  - Startup migration is safe to repeat when no migrations are pending.
  - A migration failure prevents the application from being reported healthy.
  - User-facing errors do not expose connection strings, SQL, stack traces, or provider details.
  - The single-instance MVP deployment assumption is documented.
  - `Program.cs` coordinates startup configuration validation, pending migration execution, and readiness; no unrelated service files are included.

## Phase 5: Azure Deployment Verification

**Purpose**: Verify the deployed App Service, startup migration, authentication, user ownership, persistence, and redeployment behavior in the single production environment.

### T021 - Configure protected Azure production database settings

- **Files affected**: Azure App Service configuration, `specs/003-azure-database-deployment/quickstart.md`, `README.md`
- **Dependencies**: T004, T020
- **Traceability**: [FR-026] [FR-027] [FR-028] [FR-029] [FR-030] [NFR-005] [NFR-008]
- **Acceptance criteria**:
  - The Azure App Service supplies the MySQL In-App connection through protected configuration.
  - The application resolves the setting as `ConnectionStrings:DefaultConnection` or the documented equivalent.
  - No production credential is present in repository files or published artifacts.
  - Azure configuration does not select LocalDB or development settings.
  - The single production instance and MySQL In-App limitation are documented.

### T022 - Verify Azure startup migration execution

- **Files affected**: `specs/003-azure-database-deployment/contracts/deployment-contract.md`, Azure deployment logs, smoke-test artifacts
- **Dependencies**: T020, T021
- **Traceability**: [FR-021] [FR-022] [FR-037] [FR-038] [AC-008] [AC-009] [SC-005] [SC-007] [NFR-001] [NFR-009]
- **Acceptance criteria**:
  - A deployment with a pending migration starts the application and applies that migration.
  - A deployment with no pending migration starts successfully without destructive changes.
  - Migration success or failure is observable in authorized diagnostics without secret leakage.
  - The application is not considered healthy until startup migration and database connectivity succeed.

### T023 - Execute production smoke testing

- **Files affected**: `specs/003-azure-database-deployment/contracts/deployment-contract.md`, `specs/003-azure-database-deployment/quickstart.md`, smoke-test documentation/artifacts
- **Dependencies**: T021, T022
- **Traceability**: [FR-001] [FR-004] [FR-005] [FR-010] [FR-012] [FR-014] [FR-021] [FR-038] [FR-039] [AC-009] [AC-011] [AC-012] [SC-007] [SC-011] [NFR-001] [NFR-003] [NFR-005]
- **Acceptance criteria**:
  - The deployed application starts at the intended `PlantwiseWaterTracking` App Service.
  - Smoke testing covers registration, login, logout, password validation, protected-page access, and user-data save/retrieval.
  - Smoke testing confirms that one user cannot access another user's data.
  - A failed smoke test causes the GitHub Actions workflow to report failure.
  - Test accounts and data are identified and handled without exposing credentials.

### T024 - Verify persistence after App Service restart

- **Files affected**: `specs/003-azure-database-deployment/quickstart.md`, deployment verification records
- **Dependencies**: T023
- **Traceability**: [FR-024] [AC-006] [SC-003] [NFR-001] [NFR-004]
- **Acceptance criteria**:
  - A user account and saved MVP data created before restart remain available afterward.
  - The user can log in again and retrieve the same records.
  - Startup migration does not recreate or erase existing data.

### T025 - Verify persistence after redeployment

- **Files affected**: `.github/workflows/master_plantwisewatertracking.yml`, `specs/003-azure-database-deployment/quickstart.md`, deployment verification records
- **Dependencies**: T019, T023, T024
- **Traceability**: [FR-024] [FR-035] [AC-007] [SC-004] [NFR-001] [NFR-004]
- **Acceptance criteria**:
  - A later successful deployment preserves existing user accounts and saved data.
  - Startup migration safely recognizes already-applied migrations.
  - The post-deployment smoke test passes against the redeployed artifact.
  - The workflow records the release commit, artifact, migration state, and persistence result.

### T026 - Verify release failure and recovery behavior

- **Files affected**: `.github/workflows/master_plantwisewatertracking.yml`, `specs/003-azure-database-deployment/contracts/deployment-contract.md`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T020, T019, T025
- **Traceability**: [FR-034] [FR-037] [FR-039] [AC-009] [AC-012] [SC-006] [SC-007] [SC-010] [SC-011] [NFR-009] [NFR-010]
- **Acceptance criteria**:
  - A failed test blocks publishing or deployment.
  - A migration validation failure blocks publishing or deployment.
  - A startup migration failure is detected by post-deployment verification.
  - Recovery does not reset or delete the production database.
  - A compatible application artifact can be restored without destructive schema rollback.

### T027 - Validate database-unavailable and migration-failure behavior end to end

- **Files affected**: `Touching_Grass/Program.cs`, `tests/Touching_Grass.UnitTests/DatabaseFailureTests.cs`, `specs/003-azure-database-deployment/contracts/deployment-contract.md`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T020, T021, T023
- **Traceability**: [FR-023] [FR-028] [FR-034] [FR-037] [FR-038] [FR-039] [AC-012] [SC-010] [SC-011] [NFR-003] [NFR-005] [NFR-009]
- **Acceptance criteria**:
  - Missing configuration, invalid credentials, unreachable database, and migration failure are tested as distinct negative cases.
  - Startup/readiness fails and the required smoke-test step reports failure to GitHub Actions for each case.
  - The application never silently falls back to SQL Server LocalDB and never calls `EnsureCreated`.
  - User-facing output is generic; authorized logs identify the failure stage without credentials, connection strings, SQL, stack traces, or personal data.

### T028 - Validate WCAG 2.1 AA accessibility for authentication and user-data flows

- **Files affected**: `Touching_Grass/Pages/Account/Register.cshtml`, `Touching_Grass/Pages/Account/Login.cshtml`, `Touching_Grass/Pages/Account/Logout.cshtml`, `Touching_Grass/Pages/Account/Profile.cshtml`, `Touching_Grass/Pages/Index.cshtml`, `Touching_Grass/Pages/MyPlants/Index.cshtml`, `Touching_Grass/Pages/MyPlants/Details.cshtml`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T008, T009, T010, T013, T014, T023
- **Traceability**: [FR-040] [AC-013] [SC-012] [NFR-007]
- **Acceptance criteria**:
  - Registration, login, logout, profile, ZIP-code, and saved-plant/favorite flows work by keyboard only.
  - Form controls have programmatic labels; validation messages are understandable and associated with fields.
  - Focus is visible, errors are identified, contrast is reasonable, and authentication does not depend on color alone.
  - Results are recorded as a small WCAG 2.1 AA MVP checklist without introducing enterprise accessibility tooling.

### T029 - Measure representative MVP performance

- **Files affected**: `specs/003-azure-database-deployment/quickstart.md`, `specs/003-azure-database-deployment/contracts/deployment-contract.md`, `specs/003-azure-database-deployment/evidence/[NEW if intentionally committed]`
- **Dependencies**: T023, T024
- **Traceability**: [FR-038] [AC-014] [SC-009] [NFR-002]
- **Acceptance criteria**:
  - In a production-like Azure environment, measure home, registration, login, authenticated profile/dashboard, and one representative save-and-retrieve operation.
  - Run at least 10 timed end-to-end page/request measurements per operation after excluding warm-up runs.
  - At least 95% of measurements for each operation complete within 3 seconds.
  - Record environment, run count, warm-up handling, measured results, and pass/fail outcome; do not introduce enterprise load-testing infrastructure.

### T030 - Verify required smoke-test data handling and evidence locations

- **Files affected**: `.github/workflows/master_plantwisewatertracking.yml`, `specs/003-azure-database-deployment/contracts/deployment-contract.md`, `specs/003-azure-database-deployment/quickstart.md`, `specs/003-azure-database-deployment/evidence/[NEW if intentionally committed]`
- **Dependencies**: T023, T027, T029
- **Traceability**: [FR-038] [FR-039] [AC-009] [SC-007] [SC-008] [NFR-005] [NFR-009]
- **Acceptance criteria**:
  - The smoke test is a required GitHub Actions step and failure marks the workflow unsuccessful.
  - It verifies startup/readiness, database reachability, successful startup migration, registration with a dedicated disposable account or safe non-destructive equivalent, login, logout, save/retrieve, and cross-user denial.
  - Test data is cleaned up where safe or isolated to dedicated disposable identities; credentials are stored only in protected configuration.
  - Evidence is mapped to GitHub Actions logs and uploaded artifacts if used, Azure App Service logs, and sanitized committed evidence only under the documented repository evidence path.
  - No secrets, raw connection strings, or sensitive logs are committed or retained in test artifacts.

## Dependencies and Execution Order

### Phase dependencies

- **Phase 1** has no feature dependencies and must complete before database-backed implementation.
- **Phase 2** depends on Phase 1 because Identity requires the MySQL provider and migration model.
- **Phase 3** depends on Phase 2 because user-owned data depends on authenticated users and the Identity relationship.
- **Phase 4** depends on Phases 1–3 because CI must validate the completed provider, migrations, tests, and ownership behavior.
- **Phase 5** depends on Phase 4 because Azure verification requires the gated deployment workflow.
- **T027** depends on startup/readiness and smoke behavior; **T028** depends on all affected UI flows; **T029** depends on representative deployed flows; **T030** depends on smoke, failure, and performance evidence definitions.

### Parallel opportunities

- T004 can run in parallel with T002 after T001.
- T008 and T009 require sequential Identity behavior, but test fixtures and documentation can be prepared in parallel after T007.
- T013 and T014 can be developed in parallel after T012 if they use separate files.
- T017 and T018 can be prepared in parallel after the foundational test and migration validation behavior exists.
- T021 documentation can be prepared in parallel with T019, but Azure verification must wait for the deployment gate.

## Independent Validation Criteria

- **Registration**: T008 proves valid, invalid, duplicate, and password-rule registration behavior.
- **Login**: T009 proves valid and invalid authentication against persisted Identity records.
- **Logout**: T010 proves protected access is unavailable after logout.
- **User ownership**: T015 proves two-user isolation and identifier-tampering protection.
- **Database persistence**: T011, T014, and T016 prove Identity and user data survive process restart and redeployment.
- **Startup migration**: T020 and T022 prove pending migrations execute during startup and failures prevent health success.
- **Redeployment persistence**: T025 proves records remain available after a later deployment.
- **Smoke testing**: T023 proves the deployed App Service supports the complete MVP flow.
- **Database failure behavior**: T027 proves missing configuration, invalid credentials, unreachable database, and migration failure fail startup/readiness and smoke verification safely.
- **Accessibility**: T028 proves WCAG 2.1 AA MVP coverage for authentication and user-data experiences.
- **Performance**: T029 proves the defined 3-second measurement target with recorded methodology.
- **Evidence handling**: T030 proves smoke-test gating and sanitized evidence mapping.

## Implementation Strategy

### MVP sequence

1. Complete Phase 1 and validate the provider/migration model.
2. Complete Phase 2 and validate registration, login, logout, and Identity persistence.
3. Complete Phase 3 and validate user ownership and saved data.
4. Complete Phase 4 and confirm tests/migration validation gate deployment.
  5. Complete Phase 5 and run Azure startup-migration, smoke, accessibility, performance, and persistence verification.

### Scope boundary

The MVP does not include email verification, password reset, social login, MFA, roles, calculation history, or migration execution from the GitHub Actions runner against production MySQL In-App.

### Task format

All tasks use sequential IDs, explicit file paths, dependency statements, and measurable acceptance criteria. No implementation code is included in this artifact.
