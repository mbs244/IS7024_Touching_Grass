# Tasks: Azure Persistent User Data

**Feature**: Azure Persistent User Data
**Specification**: [spec.md](./spec.md)
**Plan**: [plan.md](./plan.md)
**Architecture decision**: Visual Studio/local execution remains JSON-backed and in-memory with no relational database, local authentication persistence, local user-data persistence, or startup migration. Azure App Service uses protected `MYSQLCONNSTR_localdb`; Azure Identity and user-owned data persistence are required; Azure startup applies pending EF Core migrations before readiness.

## Phase 1: Environment Separation

**Purpose**: Ensure the application has an explicit database-free local mode and an Azure-only persistent mode without silently crossing environment boundaries.

### T001 - Document the revised local and Azure execution contract

- **Files affected**: `Touching_Grass/Program.cs`, `Touching_Grass/appsettings.Development.json`, `README.md`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: None
- **Traceability**: [FR-001–FR-015] [FR-025] [FR-026] [FR-028] [FR-029] [SC-013]
- **Acceptance criteria**:
  - Local execution is documented as JSON-backed and in-memory only.
  - Azure execution is documented as the only mode requiring relational persistence.
  - Development Mode does not require persistent Identity or database-backed user-data storage; Azure Mode requires both.
  - `MYSQLCONNSTR_localdb` is documented as the protected Azure connection-string contract.
  - No local relational database prerequisite is introduced.

### T002 - Implement environment-specific service registration boundaries

- **Files affected**: `Touching_Grass/Program.cs`
- **Dependencies**: T001
- **Traceability**: [FR-016] [FR-021] [FR-025] [FR-028] [FR-041] [SC-010] [SC-013]
- **Acceptance criteria**:
  - Visual Studio/local execution does not register or resolve a relational database provider.
  - Azure production requires `MYSQLCONNSTR_localdb`.
  - Missing Azure configuration is a safe startup/readiness failure.
  - Azure never falls back to local JSON/in-memory mode.
  - Automated configuration-selection checks verify Development Mode selects JSON/in-memory services and Azure Mode selects EF Core persistence.

### T003 - Preserve local JSON-backed and in-memory application behavior

- **Files affected**: existing JSON repositories/services and affected Razor Pages under `Touching_Grass/Pages/`
- **Dependencies**: T002
- **Traceability**: [FR-025] [FR-040] [SC-013]
- **Acceptance criteria**:
  - Home, plant lookup, ZIP lookup, and calculation flows run locally without a relational database.
  - Existing JSON source data remains read-only application data.
  - Local authentication and user-data persistence are not required for local readiness.
  - No local migration or database health check blocks local startup.

### T004 - Validate local startup without external database services

- **Files affected**: `tests/Touching_Grass.UnitTests/LocalExecutionTests.cs [NEW]`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T002, T003
- **Traceability**: [FR-025] [SC-013] [NFR-007]
- **Acceptance criteria**:
	- An automated ASP.NET Core integration-testing host launches the Razor Pages application with Development-mode configuration and no MySQL, SQLite, SQL Server, Docker, or other relational database.
  - Startup succeeds without `MYSQLCONNSTR_localdb`; captured startup logs/events contain no migration execution, relational connection, or migration-history activity.
  - Automated HTTP requests to representative JSON-backed home, plant lookup, ZIP lookup, and calculation pages return successful responses and complete their representative local flow.
  - The recorded evidence includes successful host startup, successful representative HTTP responses, absence of migration execution logging, and no relational database dependency.

## Phase 2: Azure Persistence Model

**Purpose**: Preserve ASP.NET Core Identity and user-owned data persistence only for Azure execution.

### T005 - Confirm the Azure MySQL provider and connection contract

- **Files affected**: `Touching_Grass/Touching_Grass.csproj`, `Touching_Grass/Program.cs`, `specs/003-azure-database-deployment/research.md`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T001
- **Traceability**: [FR-017] [FR-026] [FR-029] [FR-030] [NFR-007] [NFR-008]
- **Acceptance criteria**:
  - Oracle `MySql.EntityFrameworkCore` 10.0.12 and compatible EF Core 10.x packages are documented.
  - Azure MySQL In-App compatibility is treated as an Azure validation gate.
  - `MYSQLCONNSTR_localdb` is the only approved production connection source.
  - No production secret is added to the repository.

### T006 - Register Azure-only ASP.NET Core Identity persistence

- **Files affected**: `Touching_Grass/Program.cs`, `Touching_Grass/Models/ApplicationUser.cs`, `Touching_Grass/Data/ApplicationDbContext.cs`
- **Dependencies**: T002, T005
- **Traceability**: [FR-001] [FR-002] [FR-003] [FR-008] [FR-018] [FR-020] [SC-001]
- **Acceptance criteria**:
  - Azure uses ASP.NET Core Identity with `ApplicationDbContext` and the MySQL provider.
  - Identity records persist in Azure rather than process memory or JSON.
  - Local execution does not require the Identity EF store to be reachable.
  - Email verification, password reset, social login, MFA, and roles remain out of scope.

### T007 - Preserve the Azure user-owned data model and ownership rules

- **Files affected**: `Touching_Grass/Models/UserPlant.cs`, `Touching_Grass/Data/ApplicationDbContext.cs`, `Touching_Grass/Services/UserPlantService.cs`, `specs/003-azure-database-deployment/data-model.md`
- **Dependencies**: T006
- **Traceability**: [FR-009] [FR-010] [FR-011] [FR-012] [FR-013] [FR-014] [FR-018] [FR-019] [AC-005] [SC-002]
- **Acceptance criteria**:
  - Azure-persisted records have exactly one owner.
  - Create, read, update, and delete operations derive and filter ownership from the authenticated principal.
  - Cross-user access is denied without revealing record existence.
  - Default ZIP, saved plants, favorites, snapshots, timestamps, indexes, and uniqueness constraints remain represented.

### T008 - Validate custom Identity Razor Pages in Azure mode

- **Files affected**: `Touching_Grass/Pages/Account/Register.cshtml`, `Register.cshtml.cs`, `Login.cshtml`, `Login.cshtml.cs`, `Logout.cshtml`, `Logout.cshtml.cs`, `Profile.cshtml`, `Profile.cshtml.cs`, tests
- **Dependencies**: T006, T007
- **Traceability**: [FR-001] [FR-004] [FR-005] [FR-006] [FR-007] [FR-040] [AC-001] [AC-002] [AC-003] [SC-001]
- **Acceptance criteria**:
  - Existing custom pages under `Touching_Grass/Pages/Account` are used.
  - Registration, login, logout, password validation, and profile behavior operate against Azure persistence.
  - Unauthenticated access is denied or redirected safely.
  - No `Areas/Identity/Pages` or MVC controller implementation is invented unless explicitly required.

## Phase 3: Azure Migration Lifecycle

**Purpose**: Keep EF Core migrations and startup migration execution Azure-only.

### T009 - Archive incompatible historical migrations

- **Files affected**: `Touching_Grass/Data/Migrations/`, `Touching_Grass/Data/Migrations.SqlServerArchive/`
- **Dependencies**: T005
- **Traceability**: [FR-017] [FR-020] [NFR-006]
- **Acceptance criteria**:
  - Historical SQL Server migrations remain preserved outside the active runtime migration set.
  - Local execution does not discover or execute migrations.
  - Azure discovers only the validated MySQL migration set.

### T010 - Generate and review the Azure MySQL migration set

- **Files affected**: `Touching_Grass/Data/Migrations/`, `Touching_Grass/Data/ApplicationDbContext.cs`
- **Dependencies**: T006, T007, T009
- **Traceability**: [FR-018] [FR-019] [FR-020] [FR-022] [AC-008]
- **Acceptance criteria**:
  - Generated migrations include Identity, `ApplicationUser`, `UserPlant`, ownership, indexes, uniqueness, and migration history.
  - SQL Server-specific operations are not used in the active Azure set.
  - No alternate local relational migration set is added.
  - Generated artifacts are produced through EF tooling rather than hand-translated.

### T011 - Execute migrations only during Azure startup

- **Files affected**: `Touching_Grass/Program.cs`, `tests/Touching_Grass.UnitTests/StartupMigrationTests.cs [NEW]`
- **Dependencies**: T002, T010
- **Traceability**: [FR-021] [FR-022] [FR-023] [FR-024] [FR-027] [FR-028] [AC-008] [AC-010] [AC-012] [SC-010] [SC-011]
- **Acceptance criteria**:
  - Azure startup validates `MYSQLCONNSTR_localdb` and applies pending migrations before readiness.
  - Local startup does not execute migrations.
  - Startup never calls `EnsureCreated` and never automatically rolls back production migrations.
  - Missing configuration, unreachable database, invalid credentials, and migration failure prevent Azure readiness.
  - User-facing errors remain generic and diagnostics are redacted.

### T012 – Conditionally validate MySQL migrations against a configured disposable MySQL-compatible validation target

- **Files affected**: `tests/Touching_Grass.UnitTests/MigrationValidationTests.cs`, `.github/workflows/master_plantwisewatertracking.yml`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T010, T011
- **Traceability**: [FR-017] [FR-020] [FR-034] [FR-037] [AC-008] [SC-005] [SC-007]
- **Description**: When a configured disposable MySQL-compatible validation target is present, validate clean initialization, Identity and application schema creation, supported upgrade behavior, and safe migration reruns. When no configured disposable MySQL-compatible validation target is present, record the external validation as skipped. A recorded skip does not block deployment, but Azure startup migration, readiness verification, and post-deployment smoke testing remain mandatory.
- **Acceptance criteria**:
  - Configured target present: validation runs.
  - Configured target present and validation fails: publishing and deployment are blocked.
  - No configured target: validation is recorded as skipped.
  - Recorded skip: deployment may continue.
  - Clean initialization and safe rerun assertions apply only when the configured target is present.
  - Azure startup migration, readiness, and smoke testing remain mandatory in every release.
  - CI never applies migrations to production.
  - Local-mode validation explicitly confirms that no migration is attempted.

## Phase 4: Azure CI/CD and Verification

**Purpose**: Gate deployment and verify Azure persistence without making local execution database-dependent.

### T013 - Gate Azure publishing on tests and migration validation

- **Files affected**: `.github/workflows/master_plantwisewatertracking.yml`
- **Dependencies**: T004, T008, T012
- **Traceability**: [FR-031] [FR-032] [FR-033] [FR-034] [FR-035] [FR-036] [SC-006] [NFR-009]
- **Acceptance criteria**:
	- Checkout, SDK setup, restore, build, automated tests, local-mode validation, optional migration validation, and configuration checks run before publish/deployment.
	- If a configured disposable MySQL-compatible validation target is present and validation fails, deployment is blocked; if no such target is present, validation is recorded as skipped and deployment may continue.
  - The workflow contains no production connection string or password.

### T014 - Configure protected Azure production settings

- **Files affected**: Azure App Service configuration, `README.md`, `specs/003-azure-database-deployment/quickstart.md`
- **Dependencies**: T005, T011
- **Traceability**: [FR-026] [FR-027] [FR-028] [FR-029] [FR-030]
- **Acceptance criteria**:
  - `PlantwiseWaterTracking` provides `MYSQLCONNSTR_localdb` through protected configuration.
  - Azure does not select local JSON/in-memory mode.
  - Production credentials are absent from source, artifacts, and ordinary logs.
  - The one-instance Azure migration limitation is documented.

### T015 - Execute the required Azure smoke test

- **Files affected**: `.github/workflows/master_plantwisewatertracking.yml`, smoke-test documentation/artifacts, deployment contract
- **Dependencies**: T011, T013, T014
- **Traceability**: [FR-001] [FR-002] [FR-003] [FR-004] [FR-005] [FR-006] [FR-007] [FR-008] [FR-010] [FR-012] [FR-014] [FR-015] [FR-038] [FR-039] [AC-009] [AC-011] [AC-012] [SC-001] [SC-002]
- **Acceptance criteria**:
  - The smoke test is required and failure marks the workflow unsuccessful.
  - It verifies readiness after startup migration, then registers an isolated first test account, logs in, saves a favorite plant, retrieves it, and confirms the data remains present.
  - It registers or uses a separate isolated second test account and verifies that the second account cannot retrieve or mutate the first account's favorite; the response does not reveal whether the record exists.
	- Test accounts use unique run-scoped identities and protected credentials; the preferred cleanup deletes the created test data and test accounts when deployment permissions permit, and otherwise records the isolated test identifiers and evidence for controlled cleanup.
  - The flow proves Azure persistence because the saved record is read back through a later authenticated request, and proves ownership enforcement because the second account is denied access.
  - Test identities and credentials use protected configuration and sanitized evidence.

### T016 - Verify Azure restart and redeployment persistence

- **Files affected**: deployment contract, `quickstart.md`, sanitized evidence path
- **Dependencies**: T015
- **Traceability**: [FR-024] [AC-006] [AC-007] [SC-003] [SC-004]
- **Acceptance criteria**:
  - Azure accounts and user-owned data survive a controlled App Service restart.
  - Data survives a later successful deployment.
  - Startup migration recognizes already-applied migrations without resetting data.

### T017 - Validate Azure failure, accessibility, performance, and evidence requirements

- **Files affected**: `quickstart.md`, deployment contract, sanitized evidence path
- **Dependencies**: T015, T016
- **Traceability**: [FR-023] [FR-034] [FR-038] [FR-039] [FR-041] [FR-042] [FR-043] [SC-008] [SC-009] [SC-010] [SC-011] [SC-012]
- **Acceptance criteria**:
  - Azure missing configuration, invalid credentials, unreachable database, and migration failure produce failed readiness and smoke verification.
  - WCAG 2.1 AA MVP checks cover the custom authentication and user-data pages.
  - Representative Azure performance measurements use at least 10 post-warm-up runs per operation and the defined threshold.
  - Evidence is sanitized and does not contain secrets, raw connection strings, SQL, stack traces, or personal data.

## Phase 5: LocalUserPlantService Concurrency Safety (High Priority Addendum)

**Purpose**: Eliminate high-severity race conditions in Development-mode in-memory user-plant operations while preserving per-user consistency.

### T018 - Define and document the per-user synchronization strategy for local user-plant operations

- **Files affected**: `specs/003-azure-database-deployment/plan.md`, `specs/003-azure-database-deployment/tasks.md`
- **Dependencies**: T003
- **Traceability**: [FR-044] [AC-013]
- **Acceptance criteria**:
  - The concurrency issue and root cause are documented as compound-operation races not solved by `ConcurrentDictionary` alone.
  - The design requires one shared per-user lock boundary for upsert, toggle, and read/clone operations.
  - The strategy explicitly preserves concurrency across different users.

### T019 - Apply per-user locking across upsert, toggle, and read/clone paths in local service

- **Files affected**: `Touching_Grass/Services/LocalUserPlantService.cs`
- **Dependencies**: T018
- **Traceability**: [FR-044] [AC-013]
- **Acceptance criteria**:
  - `UpsertAfterRecommendationAsync` executes lookup + insert/update atomically per user.
  - `ToggleFavoriteAsync` executes read-modify-write atomically per user.
  - `ListAsync` and `FindAsync` clone from a consistent per-user view.
  - No global lock is introduced.

### T020 - Add concurrency-focused unit tests for duplicate-prevention and toggle consistency

- **Files affected**: `tests/Touching_Grass.UnitTests/LocalUserPlantServiceConcurrencyTests.cs [NEW]`
- **Dependencies**: T019
- **Traceability**: [FR-044] [AC-013]
- **Acceptance criteria**:
  - Repeated concurrent upserts for the same user/source plant do not create duplicates.
	- For a known number of concurrent toggle operations, final favorite state matches toggle parity (even toggles preserve initial state; odd toggles invert it).
  - Concurrent toggle operations complete without corrupted state or runtime concurrency exceptions.
  - Assertions target invariants and avoid timing-fragile assumptions.

### T021 - Add concurrency-focused unit tests for consistent read/clone behavior under mutation

- **Files affected**: `tests/Touching_Grass.UnitTests/LocalUserPlantServiceConcurrencyTests.cs`
- **Dependencies**: T019
- **Traceability**: [FR-044] [AC-013]
- **Acceptance criteria**:
  - Concurrent read operations (`ListAsync`, `FindAsync`) under mutation return consistent snapshots.
  - Returned objects are clones and external mutations do not alter stored records.

### T022 - Record local concurrency verification guidance and evidence expectations

- **Files affected**: `specs/003-azure-database-deployment/quickstart.md`, `README.md`
- **Dependencies**: T020, T021
- **Traceability**: [FR-044] [AC-013]
- **Acceptance criteria**:
  - Documentation states the local in-memory service is concurrency-safe per user for upsert, toggle, and read/clone operations.
  - Verification guidance includes repeated concurrent test execution and expected invariants.
  - Guidance clarifies that this addendum affects local/dev in-memory behavior only.

## Dependencies and Execution Order

- Phase 1 establishes the database-free local mode and must complete before persistence work.
- Phase 2 establishes Azure-only Identity and ownership behavior.
- Phase 3 generates and validates the Azure-only migration lifecycle; local migration execution remains prohibited.
- Phase 4 gates and verifies the Azure deployment.
- Phase 5 addresses local in-memory concurrency safety and can run after T003 in parallel with Azure-focused phases.
- T004 validates local mode independently of all Azure infrastructure.
- T019 is the implementation gate for the local concurrency addendum; T020 and T021 are required before T022 documentation closure.
- T012 validates against the configured disposable MySQL-compatible validation target, when present; T015 and T017 always validate Azure mode against deployed infrastructure, including startup migration, readiness, and smoke behavior.

## Independent Validation Criteria

- **Local mode**: T004 proves Visual Studio execution requires no relational database and does not run migrations.
- **Azure configuration**: T002, T011, and T014 prove `MYSQLCONNSTR_localdb` is required and no local fallback exists.
- **Identity**: T008 proves Azure registration, login, logout, and password rules.
- **Ownership**: T007 and T008 prove Azure user isolation and identifier-tampering protection.
- **Migrations**: T010–T012 prove Azure-only migration generation and startup execution; T012 provides external migration evidence when a configured disposable MySQL-compatible validation target is present. Otherwise, T012 records a skip, and mandatory Azure startup migration, readiness, and smoke testing provide the release evidence.
- **Release gating**: T013 blocks deployment when a configured disposable MySQL-compatible validation target fails and records a skip when no such target is present; T015 always proves Azure startup/readiness/smoke failure causes the workflow to fail.
- **Durability**: T016 proves Azure persistence across restart and redeployment.
- **Quality**: T017 proves Azure negative behavior, accessibility, performance, and evidence handling.
- **Local concurrency safety**: T019–T021 prove duplicate prevention, toggle consistency, and read/clone snapshot consistency under concurrent local requests.

## Scope Boundary

The feature does not add local authentication persistence, local user-data persistence, any local relational database provider, a dual-provider local strategy, email verification, password reset, social login, MFA, roles, calculation history, or production migration execution from the GitHub Actions runner. The concurrency addendum does not introduce global locking or alter Azure production persistence behavior.
