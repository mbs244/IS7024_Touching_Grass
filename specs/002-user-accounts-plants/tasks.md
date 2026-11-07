# Tasks: Authenticated User Accounts and Personal Plants

**Input**: Design documents from `specs/002-user-accounts-plants/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/account-and-plants.md`, `quickstart.md`

## Phase 1: Setup

**Purpose**: Establish Identity, EF Core, LocalDB configuration, and test boundaries without changing anonymous PlantWise behavior.

- [X] T001 Inspect `Touching_Grass/Touching_Grass.csproj` and the existing `tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj` before selecting dependencies. Confirm both target `net10.0`, inspect the installed .NET SDK and existing package references, confirm the existing xUnit test project is included in `Touching_Grass.slnx` and references the application project, and reuse it without creating a duplicate project or changing its target framework unless an approved compatibility decision requires it. For the application project, select compatible versions only after inspection for ASP.NET Core Identity Entity Framework store, Entity Framework Core, SQL Server provider, EF Core design-time tooling, and required EF migration tooling; document shared-framework Identity functionality instead of adding an unnecessary package. For the existing test project, retain its Microsoft.NET.Test.Sdk, xUnit, xUnit runner, and coverlet dependencies and add only directly required ASP.NET Core integration-testing or EF test-database support. Do not hard-code or guess versions, keep dependencies compatible with the installed SDK and `net10.0`, restore and build, resolve conflicts without downgrading the target framework, and record selected package names/versions in implementation notes or `quickstart.md`. Requirements: FR-001a, FR-003, FR-005, FR-011, FR-025.
- [X] T002 [P] Add a LocalDB development connection string and non-secret Identity/database configuration to `Touching_Grass/appsettings.json` and development configuration. Requirements: FR-005, FR-011, FR-025.
- [ ] T003 [P] Add isolated SQL Server LocalDB test-database configuration and disposable test-fixture support under `tests/Touching_Grass.UnitTests/`. Use a unique test database name, apply the application project's migration, begin each execution from a known state, prevent state leakage between users and cases, and ensure the fixture never connects to or modifies the ordinary developer database. Requirements: FR-011, FR-023, SC-003.
- [ ] T004 [P] Update `.gitignore` and README configuration guidance to exclude connection strings with secrets, local databases, and generated development artifacts. Requirements: FR-025.
- [X] T005 [P] Preserve existing JSON copy configuration and document that `plants.json` and `zip-locations.json` remain read-only reference datasets. Requirements: FR-006, FR-012, FR-021.

## Phase 2: Foundational

**Purpose**: Implement shared Identity, relational persistence, ownership, and source-data boundaries that block all user stories.

- [X] T006 Create `Touching_Grass/Models/ApplicationUser.cs` using ASP.NET Core Identity with one validated default ZIP property and no formal Identity roles. Requirements: FR-001a, FR-005, FR-022, FR-023.
- [X] T007 Create `Touching_Grass/Models/UserPlant.cs` with authenticated owner ID, SourcePlantId, CommonNameSnapshot, ScientificNameSnapshot, IsFavorite, CreatedAt, and LastSearchedAt as UTC timestamps. Set CreatedAt only on first successful addition; set/update LastSearchedAt only after successful recommendations; never copy water factor as authoritative data. Requirements: FR-010, FR-012, FR-014, FR-016, FR-028.
- [X] T008 Create `Touching_Grass/Data/ApplicationDbContext.cs` using the Identity Entity Framework store and map unique normalized email and unique `(UserId, SourcePlantId)` constraints. Requirements: FR-001b, FR-012, FR-023.
- [X] T009 Register Identity, `ApplicationDbContext`, SQL Server, authentication, authorization, and feature services in `Touching_Grass/Program.cs`. Do not register Identity roles. Requirements: FR-002, FR-022, FR-023.
- [X] T010 From `Touching_Grass/Touching_Grass.csproj`, use the application `ApplicationDbContext` to create the initial Entity Framework Core migration under `Touching_Grass/Data/Migrations/`. Include ASP.NET Core Identity entities, `ApplicationUser` default ZIP profile data, `UserPlant` authenticated ownership, `SourcePlantId`, `CommonNameSnapshot`, `ScientificNameSnapshot`, `IsFavorite`, `CreatedAt`, `LastSearchedAt`, foreign-key relationships, and the unique `(UserId, SourcePlantId)` constraint; do not model `plants.json` or `zip-locations.json` as relational tables. Document normal SQL Server LocalDB database creation/update from the application project and the isolated integration-test procedure: tests apply this same application-owned migration to a unique SQL Server LocalDB test database, do not own or copy migrations, never use ordinary development or production data, start from a known state, and verify repeated runs do not depend on prior state. Requirements: FR-005, FR-011, FR-012, FR-023, SC-003, SC-007.
- [X] T011 Implement `Touching_Grass/Services/IUserPlantService.cs` and `UserPlantService.cs` for owner-scoped list, upsert, favorite toggle, source resolution, and recommendation orchestration. Requirements: FR-010, FR-012, FR-015, FR-018, FR-023.
- [X] T012 Extend `IZipLookupService`/`ZipLookupService` and `IPlantRepository`/`PlantRepository` boundaries for validated ZIP resolution and current source-plant resolution while leaving JSON datasets read-only. Requirements: FR-006, FR-012, FR-014, FR-024.
- [X] T013 Define the recommendation boundary so saved plants call the existing `WateringCalculatorService` with the existing deterministic mock-weather inputs; do not add external weather providers. Requirements: FR-018, FR-019, FR-026, SC-008.
- [ ] T014 Add foundational tests in the existing `tests/Touching_Grass.UnitTests/` project for Identity password delegation, explicit password-policy enforcement (shorter than 8 characters, missing digit/lowercase/uppercase rejection, valid password without a non-alphanumeric character), password non-echoing, generic authentication failures, duplicate email handling, persistence uniqueness, ZIP validation, owner scoping, and no formal roles. Requirements: FR-003, FR-004, FR-006, FR-012, FR-022, FR-023, FR-025.

## Phase 3: User Story 1 - Register an account (P1)

**Goal**: A visitor can create an email/password account with a valid default ZIP.

**Independent Test**: Register with a supported ZIP and verify persistence; repeat with an unknown/malformed ZIP and duplicate email and verify safe rejection with no partial state.

- [ ] T015 [P] [US1] Create `Pages/Account/Register.cshtml.cs` using the approved ASP.NET Core Identity account workflow and server-side email-address validation. Trim leading/trailing email whitespace before validation, use Identity normalized-email comparison, require email, Password, ConfirmPassword, and default ZIP, reject malformed email before account creation, and persist no partial account/profile data after any validation failure. Requirements: FR-001, FR-001a, FR-003, FR-006, FR-024.
- [ ] T016 [P] [US1] Create `Pages/Account/Register.cshtml` with required email, password, ConfirmPassword, and default ZIP fields, accessible labels, validation summary, safe field-level messages, password-type inputs, and no password redisplay after failed submission. Requirements: FR-001, FR-003, FR-024.
- [ ] T017 [US1] Ensure registration uses Identity password hashing/verification and normalized-email behavior, never echoes or logs Password/ConfirmPassword, never redisplays either value after failed submission, and returns a generic already-registered message for duplicate normalized email. Requirements: FR-001a, FR-001b, FR-003, FR-004, FR-025.
- [ ] T018 [US1] Add `IdentityAccountTests.cs` for missing/malformed/trimmed/valid email, duplicate normalized email, missing Password or ConfirmPassword, password-confirmation mismatch, Identity policy failure, valid matching credentials, password non-echoing/non-redisplay, and no-partial-save behavior for account, ZIP, and profile data. Requirements: FR-001, FR-001a, FR-001b, FR-003, FR-006, FR-024, FR-025, SC-007.

## Phase 4: User Story 2 - Sign in and sign out privately (P1)

**Goal**: Registered users can authenticate, sign out, and access only protected personal areas.

**Independent Test**: Sign in successfully, access a protected page, sign out, and verify denial/redirect; use invalid credentials and verify generic failure.

- [ ] T019 [P] [US2] Create `Pages/Account/Login.cshtml.cs` and `Login.cshtml` using Identity sign-in with generic failure messaging. Requirements: FR-002, FR-004, FR-025.
- [ ] T020 [P] [US2] Create `Pages/Account/Logout.cshtml.cs` and `Logout.cshtml` with authenticated sign-out and antiforgery-protected POST semantics. Requirements: FR-002, FR-022.
- [ ] T021 [US2] Apply authorization to `Profile`, `MyPlants/Index`, and `MyPlants/Details` and ensure redirects do not disclose personal data. Requirements: FR-022, FR-023.
- [ ] T022 [US2] Add authentication tests for successful sign-in, invalid credentials, sign-out, signed-out access, and no account-existence disclosure. Requirements: FR-002, FR-004, FR-022, FR-025, SC-002, SC-003.

## Phase 5: User Story 3 - Manage the default ZIP code (P1)

**Goal**: A signed-in user can view and update one supported default ZIP used automatically for recommendations.

**Independent Test**: View and update a valid ZIP, reject invalid/unknown updates, and verify the prior valid ZIP remains after failure and the new ZIP is used later.

- [ ] T023 [P] [US3] Create `Pages/Account/Profile.cshtml.cs` with owner-scoped default-ZIP read/update behavior. Requirements: FR-005, FR-007, FR-009, FR-022, FR-023.
- [ ] T024 [P] [US3] Create `Pages/Account/Profile.cshtml` with accessible ZIP input, validation summary, and safe success/error feedback. Requirements: FR-006, FR-007, FR-024.
- [ ] T025 [US3] Validate format and dataset existence before saving and preserve the prior valid ZIP after any failed update. Requirements: FR-006, FR-009, FR-024, SC-007.
- [ ] T026 [US3] Add `DefaultZipTests.cs` for persistence, unknown/malformed ZIP rejection, previous-value preservation, unavailable persisted ZIP distinction, safe profile-update recovery messaging, later valid ZIP restoration of recommendation eligibility, and owner isolation. Requirements: FR-005, FR-006, FR-007, FR-008, FR-009, FR-020, FR-023, FR-027, FR-028, SC-003, SC-007.

## Phase 6: User Story 4 - Build a personal plant list (P1)

**Goal**: A successful signed-in search creates or updates exactly one owner-scoped personal plant record.

**Independent Test**: Complete a successful search, confirm one list entry, repeat it, sign out/in, and confirm the entry and favorite state persist.

- [ ] T027 [P] [US4] Create `Pages/MyPlants/Index.cshtml.cs` with owner-scoped list loading and the single `OnPostToggleFavoriteAsync` boundary reserved for Phase 7. Requirements: FR-011, FR-013, FR-016, FR-023.
- [ ] T028 [P] [US4] Create `Pages/MyPlants/Index.cshtml` with empty state, current/snapshot plant names, unavailable-source state, and accessible navigation. Requirements: FR-013, FR-014, FR-017.
- [ ] T029 [US4] Integrate the existing `Pages/Index.cshtml.cs` successful search so upsert occurs only after ZIP resolution, plant resolution, and successful `WateringCalculatorService` calculation, before the result response/redirect. Requirements: FR-010, FR-018, FR-027, SC-004, SC-007.
- [ ] T030 [US4] Implement relational upsert preserving `CreatedAt` and `IsFavorite`, enforcing `(UserId, SourcePlantId)` uniqueness, and setting/updating UTC `LastSearchedAt` only after a successful recommendation. Viewing the list, toggling favorite, unavailable-source rendering, failed validation, lookup, mock-weather processing, or calculation must not change `LastSearchedAt`. Requirements: FR-012, FR-016, FR-027, FR-028, SC-004, SC-007.
- [ ] T031 [US4] Implement unavailable-source rendering from retained snapshots; preserve the record/favorite state, disable recommendation opening, and never calculate with stale snapshot data. Requirements: FR-014, FR-020, FR-028, SC-007.
- [ ] T032 [US4] Add `UserPlantServiceTests.cs` and `UnavailableSourcePlantTests.cs` confirming first successful recommendation sets both `CreatedAt` and `LastSearchedAt`, repeated successful recommendations preserve `CreatedAt` and update only `LastSearchedAt`, favorite changes preserve both timestamps, failed recommendations preserve both timestamps and create no record, list and details views do not modify either timestamp, retained snapshots render safely, and stale calculation is prevented. Requirements: FR-010, FR-012, FR-014, FR-027, FR-028, SC-004, SC-007.
- [ ] T033 [US4] Add `CrossUserIsolationTests.cs` for list isolation and unauthorized create/update attempts. Requirements: FR-022, FR-023, SC-003, SC-007.

## Phase 7: User Story 5 - Favorite personal plants (P2)

**Goal**: A signed-in user can toggle owned plants as favorites with persistent favorites-first ordering.

**Independent Test**: Favorite and unfavorite saved plants, verify server-rendered status/order, repeat after sign-in, and attempt a cross-user toggle.

- [ ] T034 [P] [US5] Implement only `OnPostToggleFavoriteAsync` in `Pages/MyPlants/Index.cshtml.cs`; require authentication, antiforgery, server-controlled UserPlant ID, owner-scoped query, atomic persistence, and redirect to the list. Requirements: FR-015, FR-016, FR-022, FR-023.
- [ ] T035 [US5] Render the favorite control in `Pages/MyPlants/Index.cshtml` as a keyboard-operable server-rendered button with visible/accessibility text `Add [plant] to favorites` or `Remove [plant] from favorites`; do not rely on color/icon shape. Requirements: FR-017, FR-024.
- [ ] T036 [US5] Enforce favorites-first ordering with stable ordering within each group. Requirements: FR-016.
- [ ] T037 [US5] Add `FavoriteTests.cs` for toggle, repeated toggle, persistence, ordering, accessible state requirements, and cross-user denial without data disclosure. Requirements: FR-015, FR-016, FR-017, FR-023, FR-025, SC-003, SC-005, SC-007.

## Phase 8: User Story 6 - Open a saved plant recommendation (P1)

**Goal**: A signed-in user can open an owned saved plant and receive a current mock-weather recommendation using the current default ZIP and established calculator.

**Independent Test**: Open a saved plant and verify content, change the default ZIP and verify recalculation, and compare the saved result with the anonymous search result using identical inputs.

- [ ] T038 [P] [US6] Create `Pages/MyPlants/Details.cshtml.cs` with owner-scoped saved-plant resolution and current default-ZIP lookup. Requirements: FR-008, FR-018, FR-022, FR-023.
- [ ] T039 [P] [US6] Create `Pages/MyPlants/Details.cshtml` displaying current common/scientific name, ZIP used, calculated water need and units, existing explanation, and mock-weather context where the UI exposes source context; omit internal IDs. Requirements: FR-019, FR-026.
- [ ] T040 [US6] Reuse `WateringCalculatorService` with the existing deterministic mock-weather inputs and do not add external weather calls. Requirements: FR-018, FR-019, FR-026, SC-008.
- [ ] T041 [US6] Add safe unavailable plant, malformed/unknown ZIP, unavailable persisted ZIP, and mock-weather error states with usable profile recovery navigation and no stack trace, database detail, personal data, or internal exception information. For an unavailable persisted ZIP, block calculation, do not invoke `WateringCalculatorService` with invented/replacement coordinates, preserve the stored ZIP, personal plant, favorite state, and timestamps, and require a later valid profile ZIP update before recommendation eligibility is restored. Requirements: FR-008, FR-020, FR-025, FR-027, FR-028, SC-007.
- [ ] T042 [US6] Add `SavedPlantRecommendationTests.cs` for content, current default ZIP, recalculation after ZIP update, unavailable persisted ZIP calculation blocking and unchanged ZIP/plant/favorite/timestamp state, profile-update recovery, later valid ZIP restoration, cross-user access denial, unavailable data, and recommendation parity with the existing anonymous search flow for identical plant/ZIP/factor/mock-weather inputs. Requirements: FR-008, FR-018, FR-019, FR-020, FR-023, FR-026, FR-027, FR-028, SC-006, SC-007, SC-008.

## Phase 9: User Story 7 - Preserve the existing visitor journey (P1)

**Goal**: Existing anonymous PlantWise search and watering behavior remains functional and unchanged.

**Independent Test**: Run valid and invalid anonymous searches and compare recommendation formula, units, and safe failures with the existing behavior.

- [ ] T043 [P] [US7] Add `AnonymousPlantWiseRegressionTests.cs` for valid search, malformed/unknown ZIP, unknown plant, formula, units, and safe unavailable-data behavior. Requirements: FR-021, SC-008.
- [ ] T044 [US7] Verify `Pages/Index.cshtml.cs`, `ZipLookupService`, `PlantRepository`, and `WateringCalculatorService` preserve anonymous behavior while adding post-calculation authenticated upsert. Requirements: FR-010, FR-021, FR-026, FR-027, SC-004, SC-008.
- [ ] T045 [US7] Verify anonymous users are not required to create an account solely to use the existing search flow. Requirements: FR-021, SC-008.

## Phase 10: Security, Performance, and Documentation

**Purpose**: Validate cross-cutting requirements, record evidence, and finish the feature without adding external weather APIs or Identity roles.

- [ ] T046 [P] Review all new logging statements, exception handlers, and serialization for passwords, password hashes, security stamps, authentication cookies, connection strings, secrets, antiforgery tokens, full account records, stack traces, database details, and unnecessary profile data; document the review. Requirements: FR-003, FR-025.
- [ ] T047 [P] Add `SafeLoggingAndErrorTests.cs` proving generic unsuccessful sign-in, password non-echoing, password hashes/security stamps not exposed, cross-user denial without data disclosure, and safe errors without connection strings, stack traces, or database details. Requirements: FR-003, FR-004, FR-023, FR-025, SC-002, SC-003, SC-007.
- [ ] T048 [P] Add `PerformanceMeasurementTests.cs` for registration and sign-in using LocalDB on the same machine, Release build, one warm-up, at least 10 executions, request-boundary timing, median/slowest reporting, one user, seeded feature data, and no startup/migration/debugger overhead; record environment, SDK/package versions, build configuration, iteration count, median, slowest result, and pass/fail. Requirements: SC-001, SC-002.
- [ ] T049 [P] Review all account and personal-plant Razor Pages for keyboard access, correct button semantics, labels, validation summaries, visible/accessibility favorite text, focus behavior, and responsive layout. Requirements: FR-017, FR-024, SC-003, SC-005.
- [ ] T050 [P] Update README and feature documentation with Identity/LocalDB setup, migration/database update instructions, mock-weather-only scope, privacy boundaries, and quickstart evidence requirements. Requirements: FR-026, FR-025, SC-001, SC-002, SC-008.
- [ ] T051 Run `quickstart.md`, verify the application-owned migration is generated under `Touching_Grass/Data/Migrations/`, confirm an empty application database and an isolated integration-test database can be created from that same migration, verify Identity/personal-plant relationships, the authenticated-user-plus-SourcePlantId unique constraint, and timestamp persistence, confirm the ordinary development database is unchanged, record environment/build/iterations/median/slowest/pass-fail performance results, document logging review, and map evidence to SC-001 through SC-008. Requirements: SC-001, SC-002, SC-003, SC-004, SC-005, SC-006, SC-007, SC-008.
- [ ] T052 Run `dotnet build Touching_Grass.slnx` and `dotnet test Touching_Grass.slnx`; resolve introduced warnings/failures before completion. Requirements: all FR-001–FR-028, SC-001–SC-008.

## Dependencies and Execution Order

- Phase 1 Setup precedes Phase 2 Foundational.
- Phase 2 blocks all user stories.
- US1 registration precedes US2 protected sign-in flows.
- US2 authentication precedes US3 profile, US4 personal list, US5 favorites, and US6 details.
- US3 default ZIP persistence is required before validating US6 recalculation.
- US4 personal-list persistence precedes US5 favorite toggles and US6 details.
- US7 regression tests run throughout and must pass before completion.
- Phase 10 depends on all desired stories and their integration tests.

## Parallel Opportunities

- T002–T005 can proceed in parallel after project dependency inspection.
- T015/T016, T019/T020, T023/T024, and T027/T028 target separate page pairs and can proceed in parallel after their foundational contracts.
- Test files marked `[P]` can proceed in parallel when they use separate fixtures/files.
- Security, accessibility, performance, and documentation reviews can proceed in parallel after the relevant user-story pages exist.

## Implementation Strategy

1. Establish Identity, EF Core, LocalDB, migrations, and test isolation.
2. Deliver registration and sign-in/sign-out with protected-page tests.
3. Deliver default ZIP and personal-list upsert with exact post-calculation timing.
4. Add favorites through the single personal-list POST handler.
5. Add saved-plant mock-weather recommendation parity and unavailable-source handling.
6. Run anonymous regression, security/logging review, accessibility review, and defined performance measurements.
7. Complete the full build/test and quickstart evidence review.
