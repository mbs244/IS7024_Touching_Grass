# Implementation Plan: Azure Persistent User Data

**Branch**: `003-azure-database-deployment` | **Date**: 2026-11-21 | **Spec**: [spec.md](./spec.md)

**Input**: Approved feature specification for replacing the local SQL Server-only configuration with Azure MySQL In-App persistence for the free MVP.

## Summary

The implementation will retain the existing .NET 10 Razor Pages and ASP.NET Core Identity architecture. Visual Studio/local execution will remain database-free and will continue using the existing JSON-backed and in-memory services; local authentication and user-data persistence are not required. Azure App Service will use Oracle `MySql.EntityFrameworkCore` 10.0.12 through the protected `MYSQLCONNSTR_localdb` connection string, persist Identity and user-owned data, and apply pending migrations during startup before readiness.

The current SQL Server migration artifacts will not be translated manually. They will be preserved in Git history and moved outside the active runtime migration directory. When a disposable MySQL-compatible validation target is configured, the Azure application model and provider-compatible migration set will be validated against it; when no target is configured, validation is recorded as skipped. In both cases, Azure startup migration, readiness, and the required smoke test remain mandatory. Production migration execution occurs only during Azure startup; local startup must not resolve a database provider or execute migrations.

## Technical Context

**Language/Version**: C# / .NET 10; nullable reference types and implicit usings enabled.

**Primary Dependencies**: ASP.NET Core Razor Pages, ASP.NET Core Identity, Entity Framework Core 10.0.x, Oracle `MySql.EntityFrameworkCore` 10.0.12, xUnit test stack, GitHub Actions Azure login and Web Apps Deploy actions.

**Storage**: Production storage uses Azure MySQL In-App. Development Mode uses the existing JSON-backed and in-memory services. A configured disposable MySQL-compatible validation target may be used for conditional external migration validation but is not required.

**Testing**: Existing xUnit unit tests plus Azure persistence integration tests for Identity, ownership, restart persistence, startup migrations, readiness, and deployment smoke verification. When a configured disposable MySQL-compatible validation target exists, integration tests also validate migration initialization, upgrade, and rerun behavior; when no target exists, validation is recorded as skipped.

**Target Platform**: Azure App Service `PlantwiseWaterTracking`, production slot, deployed from GitHub Actions after merge to `master`.

**Project Type**: Single ASP.NET Core Razor Pages web application with a separate xUnit test project.

**Performance Goals**: In a production-like Azure environment, measure the home page, registration page, login page, authenticated profile or dashboard, and one representative user-data save-and-retrieve operation. Exclude documented warm-up runs, perform at least 10 timed end-to-end measurements for each operation, and require at least 95% of measurements for each operation to complete within three seconds. Record the environment, run count, warm-up handling, results, and pass/fail outcome. This is representative MVP performance validation, not enterprise load testing.

**Constraints**: Free MVP cost target; local execution must not require MySQL, SQLite, SQL Server, Docker, or any relational database; no secrets in source control; Azure requires `MYSQLCONNSTR_localdb` with no local-mode fallback; no email verification or password reset; no social login, MFA, roles, analytics, or calculation history.

**Scale/Scope**: Small classroom/project MVP; account profile/default ZIP, saved plants/favorites, timestamps, and Identity records only. MySQL In-App operational limits must be documented and treated as a release constraint.

## Current-State Analysis

- `Touching_Grass/Program.cs` currently registers `ApplicationDbContext` with a relational provider during all environments, which conflicts with the approved database-free local execution mode.
- `Touching_Grass/appsettings.Development.json` must not introduce a relational connection string; local execution uses the existing JSON-backed and in-memory services.
- `Touching_Grass/Touching_Grass.csproj` references SQL Server EF Core and Identity EF packages.
- `Touching_Grass/Data/ApplicationDbContext.cs` contains Identity and `UserPlant` model configuration, including user ownership and uniqueness rules.
- `Touching_Grass/Data/Migrations/` contains SQL Server-specific generated migration and snapshot artifacts. They cannot be treated as MySQL migrations.
- The existing workflow builds, publishes, and deploys to Azure but does not run tests, validate/apply migrations, or perform post-deployment database verification.
- `Touching_Grass/Data/plants.json` and `zip-locations.json` remain source data files and are copied with the application; they are not the relational persistence target for this feature.

## Target-State Architecture

1. Razor Pages and existing JSON/in-memory services handle local development flows without relational persistence.
2. In Azure, Razor Pages and Identity UI receive registration, login, logout, and authenticated user-data requests backed by MySQL.
3. `ApplicationUser` stores account identity and default ZIP profile data.
4. `UserPlant` stores saved plant/favorite data, snapshots, timestamps, and an immutable user relationship.
5. Azure production connection settings are supplied by the protected App Service `MYSQLCONNSTR_localdb` connection-string setting.
6. GitHub Actions restores, builds, tests, and runs local-mode validation; when a disposable MySQL-compatible validation target is configured, it validates migrations and blocks deployment on failure, otherwise it records validation as skipped and may continue. It then publishes and deploys to `PlantwiseWaterTracking`; Azure startup migration, readiness, and required smoke verification always follow.
7. JSON plant and location source data remains separate from relational user-owned data unless a later approved feature changes that boundary.

## Technical Architecture Overview

### Environment composition boundary

`Program.cs` is the startup composition boundary. It selects the execution mode from the
ASP.NET Core hosting environment: Development uses the existing JSON-backed repositories and
in-memory services and does not register or resolve `ApplicationDbContext`; Azure production
requires `MYSQLCONNSTR_localdb`, registers EF Core Identity and user-data services, and enables
the MySQL provider. The deployment target and hosting environment, rather than a local database
choice, determine the mode. Startup migration orchestration is registered only in Azure mode,
so local startup can succeed without a relational service or migration history.

### Provider transition

- Remove the SQL Server EF Core provider from the application dependency set after the replacement is validated.
- Add Oracle `MySql.EntityFrameworkCore` 10.0.12, verified by NuGet metadata for `net10.0` and EF Core 10.0.12 dependencies. Align the Microsoft EF Core package family to the provider's compatible 10.0.x dependency during implementation.
- Change runtime provider registration from SQL Server to the selected MySQL provider.
- Validate connection options for encryption, certificate validation, timeout, retry, and pooling using the provider's supported API.
- Do not retain both providers in the production runtime unless a documented migration tool requires a temporary design-time dependency.

### Identity persistence

- Preserve ASP.NET Core Identity as the authentication boundary.
- Keep unique normalized email behavior and server-side password validation.
- Preserve the application user's default ZIP profile field.
- Keep no-email-verification behavior for the MVP.
- Exclude password reset, social login, MFA, and roles.

### Data ownership

- Preserve the required `ApplicationUser` to `UserPlant` relationship.
- Ensure create operations derive ownership from the authenticated principal.
- Ensure every read/update/delete includes the authenticated owner's identifier.
- Preserve unique source-plant-per-user behavior and audit timestamps.

## Database Architecture

- The production logical database is Azure MySQL In-App for the free MVP.
- The schema includes Identity tables, application-user profile/default ZIP data, `UserPlant`, ownership foreign-key behavior, unique constraints, indexes, and migration history.
- The existing SQL Server migrations are historical input only and must be replaced with MySQL-compatible generated migrations.
- A disposable MySQL validation target is used when configured and available; it is not required before implementation or deployment for this student project.
- The production migration must be non-destructive and must never drop or reset user data as part of routine release.
- MySQL In-App limitations around durability, backup, scaling, and availability are recorded as accepted MVP risk and revisited before growth beyond the project scope.

## Authentication Architecture

- The repository currently uses custom Razor Pages at `Touching_Grass/Pages/Account/Register.cshtml`, `Register.cshtml.cs`, `Login.cshtml`, `Login.cshtml.cs`, `Logout.cshtml`, `Logout.cshtml.cs`, `Profile.cshtml`, and `Profile.cshtml.cs`; it does not currently contain `Touching_Grass/Areas/Identity/Pages`.
- Identity UI tasks must modify these existing paths or mark any new path `[NEW]`; do not assume scaffolded `Areas/Identity` files.
- Use ASP.NET Core Identity with the existing application user type and EF store.
- Require authentication for profile and saved-plant operations.
- Use secure cookie/session defaults and HTTPS in production.
- Keep registration and login immediately usable without email verification.
- Return generic authentication and authorization failures without account or record enumeration.
- Do not add password recovery in this MVP; document it as a future feature.

## Entity Relationship Design

See [data-model.md](./data-model.md). The authoritative relationship is `ApplicationUser 1-to-many UserPlant`; `UserPlant` stores source plant identity/display snapshots and audit timestamps. The user's default ZIP remains profile data and is resolved against the existing source location dataset.

## Configuration Management Strategy

### Local development

- Do not register a relational database provider during Visual Studio/local execution.
- Continue using the existing JSON-backed and in-memory services.
- Do not require local authentication persistence, user-data persistence, MySQL, SQLite, SQL Server, Docker, or a connection string.
- Do not execute startup database migrations locally.

### Azure production

- Configure the MySQL In-App connection through the Azure App Service `MYSQLCONNSTR_localdb` connection-string setting.
- Resolve `MYSQLCONNSTR_localdb` explicitly in the Azure environment.
- Do not rely on `appsettings.Development.json` or local JSON/in-memory mode in Azure.
- Treat missing production configuration as a hard, safe startup/health failure rather than falling back to a local database.
- Verify that the configured App Service resource is `PlantwiseWaterTracking` and that the workflow and publish metadata do not target a different site.

## Azure Deployment Architecture

- GitHub Actions authenticates to Azure using the existing federated identity configuration with least privilege.
- The workflow deploys the published artifact to the production slot of `PlantwiseWaterTracking`.
- Azure App Service provides the runtime environment and protected database configuration.
- Application startup validates the protected production connection and applies pending migrations after deployment and before readiness; the workflow never connects to MySQL In-App to apply production migrations.
- Post-deployment verification confirms startup, database reachability, authentication, user-data save/retrieval, logout, and cross-user denial.
- No production secret is embedded in workflow YAML, repository files, publish output, or ordinary logs.

## CI/CD Architecture

Required job order:

1. Checkout the merged `master` commit.
2. Set up the supported .NET 10 SDK.
3. Restore dependencies.
4. Build the solution in Release mode.
5. Run the complete automated test suite.
6. Run automated local-mode validation tests against the checked-out commit.
7. Validate the MySQL migration model against a configured disposable MySQL-compatible validation target; if no such target is present, record the skipped external validation without blocking deployment.
8. Publish the application artifact.
9. Upload/download the artifact between build and deployment jobs as appropriate.
10. Authenticate to Azure.
11. Deploy to `PlantwiseWaterTracking` production.
12. Azure application startup validates `MYSQLCONNSTR_localdb`.
13. Azure application startup applies only pending migrations through Oracle MySql.EntityFrameworkCore, after which readiness becomes healthy.
14. Run the required post-deployment smoke test; fail the workflow on startup, migration, health, authentication, persistence, or authorization failure.

If a configured disposable MySQL-compatible validation target is present and validation fails, deployment is blocked. If no such target is present, validation is recorded as skipped and deployment may continue. Azure startup migration remains mandatory; startup migration failure must make readiness and the required smoke test fail, which makes the workflow unsuccessful. Diagnostic output must redact credentials and connection-string values.

## Migration Architecture

- Use the validated MySQL provider at design time and runtime.
- Generate a new MySQL migration set from the final `ApplicationDbContext` model; do not hand-edit SQL Server-generated migration operations into MySQL syntax.
- Preserve Identity and user-owned data model requirements in the model snapshot.
- When a configured disposable MySQL-compatible validation target exists, apply the migration to it, rerun it to prove idempotent release behavior, and test upgrades from the prior application state where applicable. If no target exists, record validation as skipped and continue with mandatory Azure startup migration, readiness, and smoke-test verification.
- GitHub Actions validates migration artifacts against the configured disposable MySQL-compatible validation target when present. If no target exists, the workflow records validation as skipped and continues; it never applies production migrations.
- During startup, apply only pending migrations through Oracle MySql.EntityFrameworkCore before readiness or normal authenticated/data-dependent traffic.
- Limit the MVP to one active App Service instance; disable scale-out or remain at one instance, use provider-supported migration locking when available, and reconsider startup migration before horizontal scaling.
- Do not use `EnsureCreated` or automatically roll back production migrations. Migration execution must remain idempotent and preserve EF migration history.
- Treat Azure startup migration failure as a release blocker. Treat failure against a configured disposable validation target as a deployment blocker; a missing validation target is a recorded skip. Do not reset or drop production schema to recover.

## Security Architecture

- Store only Identity-managed password hashes, never clear-text passwords.
- Protect authentication cookies and require HTTPS in production.
- Enforce ownership server-side on every user-data query and mutation.
- Use anti-forgery protection on state-changing browser requests.
- Redact connection strings, credentials, tokens, hashes, SQL details, stack traces, and personal data from logs and user responses.
- Use protected Azure configuration and federated/least-privilege CI/CD authentication.
- Validate secure MySQL transport and certificate behavior according to the selected provider and Azure platform support.
- Use generic failed-login and unavailable-record responses to reduce enumeration.

## Testing Strategy

### Unit tests

- Password and registration validation.
- Login/logout behavior and authentication state.
- User-data ownership predicates and cross-user denial.
- Data mapping and timestamp rules.
- Local-mode startup without relational services and Azure configuration selection requiring `MYSQLCONNSTR_localdb`.
- Automated configuration-selection tests prove Development mode selects JSON/in-memory services and Azure mode requires `MYSQLCONNSTR_localdb`.

### Database integration tests

#### Conditional external migration validation

When a configured disposable MySQL-compatible validation target is present, tests validate clean schema initialization, ASP.NET Core Identity schema creation, application schema creation, supported upgrade behavior, and safe migration reruns. Failure blocks publishing and deployment.

When no configured disposable MySQL-compatible validation target is present, these external checks are recorded as skipped and do not block deployment.

#### Mandatory Azure deployment verification

Every Azure release must validate production configuration, execute pending migrations during Azure App Service startup, prevent readiness until migration succeeds, and run post-deployment smoke testing. These checks are mandatory regardless of whether external validation ran or was skipped.

### CI/CD tests

- Failing unit tests block deployment.
- If a configured disposable MySQL-compatible validation target exists and migration validation fails, publishing and deployment are blocked. If no configured disposable MySQL-compatible validation target is present, validation is recorded as skipped and deployment may continue. Application-startup migration occurs after deployment inside Azure App Service; startup-migration failure prevents readiness, the required post-deployment smoke test detects the readiness failure, and the GitHub Actions workflow is unsuccessful.
- Published artifact is generated only after verification.
- Post-deployment smoke flow completes against the intended App Service.

### Manual/operational verification

- Confirm Azure App Service configuration is present and not exposed.
- Restart the app and retrieve data.
- Redeploy a later artifact and retrieve data.
- Record MySQL In-App limitations and workflow evidence in GitHub Actions logs/artifacts, Azure App Service logs, and intentionally committed repository evidence under `specs/003-azure-database-deployment/evidence/` only if sanitized evidence is deliberately retained. Never commit secrets, raw connection strings, or sensitive logs.
- Validate WCAG 2.1 AA keyboard operation, labels, validation, focus, errors, contrast, and non-color-only cues for registration, login, logout, profile, ZIP-code, and saved-plant/favorite flows.
- Measure the five defined operations—home page, registration page, login page, authenticated profile or dashboard, and one representative user-data save-and-retrieve operation—in a production-like Azure environment. Exclude documented warm-up runs, perform at least 10 timed end-to-end measurements for each operation, require at least 95% within three seconds for each operation, and record the environment, run count, warm-up handling, results, and pass/fail outcome. Keep this as representative MVP validation, not enterprise load testing.
- Run local-mode tests proving no database startup dependency, plus Azure negative tests for missing configuration, invalid credentials, unreachable database, and migration failure; verify Azure readiness and smoke-test failure, no local-mode fallback or `EnsureCreated`, generic user output, and actionable redacted logs.

## Rollback Strategy

### Application-only failure before startup migration

- Stop before deployment when restore or build fails, automated tests fail, or migration validation fails against a configured disposable MySQL-compatible validation target. If no configured disposable MySQL-compatible validation target is present, record validation as skipped and continue to mandatory Azure startup migration, readiness, and smoke verification.
- Preserve the last known-good deployed artifact and database state.

### Startup migration failure

- The application must not report readiness; the required smoke test must fail and the workflow must be unsuccessful.
- Inspect startup and App Service logs without exposing credentials.
- Correct the migration/provider/configuration issue and rerun CI validation against the configured disposable MySQL-compatible validation target, when present; otherwise verify the correction through mandatory Azure startup migration, readiness, and smoke testing.
- Do not delete migration history or reset the production database.
- Do not automatically roll back production migrations. If a partially applied migration cannot be safely repaired, use a documented data-preserving recovery or forward-fix procedure.

### Post-deployment application failure

- Revert the application artifact to the last version only when that artifact is compatible with the already-applied schema.
- Do not roll back schema destructively through an automatic workflow.
- If the schema change is not backward compatible, follow a forward-fix or approved data-preserving recovery plan.

## Risk Assessment

| Risk | Impact | Mitigation | Gate |
|------|--------|------------|------|
| MySQL provider is incompatible with .NET 10/EF Core 10 | High | Validate package compatibility and the configured disposable MySQL-compatible validation target, when present, before implementation continues | Phase 1 |
| SQL Server migrations are reused on MySQL | High | Generate a new provider-compatible migration set and inspect generated model/schema | Phase 1 |
| MySQL In-App lacks production-grade durability or backups | High | Accept only for free MVP, document limits, keep scope small, define managed-database follow-up | Phase 5 |
| Azure connection string is missing or malformed | High | Protected App Service configuration, explicit startup/health failure, negative tests | Phase 4 |
| Deployment targets wrong App Service or slot | High | Verify workflow app name, slot, subscription, and endpoint | Phase 5 |
| Migration succeeds but application model is incompatible | High | Run integration tests and post-deployment smoke verification before release completion | Phase 4/5 |
| Cross-user data access is insufficiently filtered | Critical | Ownership tests with identifier manipulation and authorization review | Phase 3 |
| CI/CD logs leak secrets | High | Redaction review and repository/workflow secret scanning | Phase 4 |
| Free MVP exceeds MySQL In-App capacity | Medium | Define expected small scope and monitor operational limits; plan managed-service migration if needed | Phase 5 |

## Implementation Phases and Verification Gates

### Phase 1 - Database provider transition

**Scope**: Confirm MySQL provider compatibility, update package references and runtime registration, isolate the production connection contract, and regenerate provider-compatible migrations.

**Entry criteria**:

- Approved specification and research decisions exist.
- Azure MySQL In-App connection format and runtime availability are confirmed.
- A configured disposable MySQL-compatible validation target is optional.
- Its absence does not prevent entry into the phase.

**Exit criteria**:

- The application has one validated MySQL-compatible EF provider.
- Runtime configuration no longer uses `UseSqlServer` for the target deployment.
- SQL Server migration artifacts are not used as MySQL production migrations.
- If a configured disposable MySQL-compatible validation target is present, external migration validation passes.
- If no configured disposable MySQL-compatible validation target is present, external migration validation is recorded as skipped.
- The migration artifacts remain ready for mandatory Azure startup verification.

**Validation tests**:

- Package restore and application build.
- Provider compatibility check against .NET 10/EF Core 10.
- When a configured disposable MySQL-compatible validation target is present, run external initialization, upgrade, and repeatability checks; otherwise record the external-validation skip and continue with mandatory Azure startup migration, readiness, and smoke-test verification.
- Configuration test proving local settings and production settings are separate.

**Rollback procedure**: Revert provider/configuration changes on the feature branch before merge; preserve validation evidence or the configured disposable MySQL-compatible validation target, when present. Do not apply unvalidated migrations to production; Azure startup migration remains the production migration path.

### Phase 2 - Identity persistence validation

**Scope**: Validate Identity table generation, account profile/default ZIP storage, registration, login, logout, password policy, and no-email-verification behavior.

**Entry criteria**:

- Phase 1 produced either a successful configured-target validation result or a recorded external-validation skip.
- ApplicationUser model and Identity configuration are identified.

**Exit criteria**:

- If the configured disposable MySQL-compatible validation target is present, the Identity and application schemas initialize successfully in that target.
- If the configured disposable MySQL-compatible validation target is absent, the external check remains recorded as skipped.
- In all cases, final Identity schema evidence must come from mandatory Azure startup migration, readiness, and smoke verification before release completion.
- Registration, login, logout, unique email, and password validation tests pass.
- No email delivery or password reset dependency is introduced.

**Validation tests**:

- Valid and invalid registration tests.
- Duplicate normalized email test.
- Valid and invalid login tests.
- Logout access-denial test.
- Identity record persistence after process restart.

**Rollback procedure**: Revert Identity configuration changes before deploying; retain the configured disposable MySQL-compatible validation target, when present, for diagnosis. Do not delete a production account database to recover from an application test failure.

### Phase 3 - Data model validation

**Scope**: Validate account profile, default ZIP, UserPlant/favorite data, timestamps, relationships, uniqueness, and server-side ownership boundaries.

**Entry criteria**:

- Phase 2 Identity persistence gate passes.
- Approved MVP data scope is represented in the model design.

**Exit criteria**:

- Every user-owned record has one owner.
- Cross-user read/update/delete attempts fail safely.
- Persistence evidence comes from the configured disposable MySQL-compatible validation target, when present; mandatory Azure startup migration, restart/redeployment verification, readiness, and smoke testing provide production behavior evidence.
- Migration schema contains required keys, constraints, and indexes.

**Validation tests**:

- Two-user ownership isolation suite.
- Direct identifier manipulation suite.
- Save/update/delete authorization suite.
- Timestamp and duplicate-source-plant tests.
- Restart persistence test.

**Rollback procedure**: Revert model changes before production migration; if a schema migration has already been applied, use only a reviewed, data-preserving forward migration or approved recovery procedure.

### Phase 4 - Migration Validation and Deployment Gating

**Scope**: Modify the existing GitHub Actions workflow so tests and MySQL migration validation run before publish/deploy; after deployment, startup migration/readiness and a required smoke test gate release success.

**Entry criteria**:

- Phases 1–3 pass locally; the configured disposable MySQL-compatible validation target passes when present, or validation is recorded as skipped when no target exists. Azure startup migration, readiness, and smoke verification remain mandatory.
- Azure secret/configuration names and permissions are documented.

**Exit criteria**:

- Full test suite runs before deployment.
- A dedicated migration-validation step runs before publish and deployment when a disposable validation target is configured; otherwise the workflow records a skipped validation.
- The validation step checks migration artifacts against the configured disposable MySQL-compatible validation target when present; otherwise it records the external validation as skipped without blocking deployment.
- The validation step does not connect to or alter production MySQL In-App.
- Production migrations execute during application startup after deployment.
- Startup migration success is confirmed through readiness and required smoke verification.
- If a configured disposable validation target exists and validation fails, publishing and deployment are blocked; if no target exists, deployment may continue. Startup-migration failure prevents readiness and is reported by the required smoke test.
- Logs are safe and workflow targets the intended App Service.

**Validation tests**:

- Passing workflow path through a controlled release candidate.
- Deliberately failing test blocks deployment.
- Deliberately invalid migration validation blocks publishing and deployment when a configured disposable MySQL-compatible validation target is present; with no configured disposable MySQL-compatible validation target, the validation is recorded as skipped. After deployment, startup configuration, credential, connectivity, or migration failure prevents readiness and fails the required smoke verification, which makes the workflow unsuccessful.
- Secret redaction review and artifact inspection.

**Rollback procedure**: Revert workflow changes to the last known-good workflow if the release path is blocked; do not bypass failed tests or migration checks to deploy.

### Phase 5 - Azure deployment verification

**Scope**: Configure Azure App Service, run the validation/publish/deployment workflow, verify application-startup production migration and readiness, and record MVP limitations and evidence.

**Entry criteria**:

- Phases 1–4 pass.
- Azure App Service `PlantwiseWaterTracking` and MySQL In-App configuration are available.
- Protected production connection configuration is present and access is authorized.

**Exit criteria**:

- Production starts without provider/configuration errors.
- Registration, login, logout, save, retrieval, and cross-user denial pass in smoke verification.
- Data survives a controlled restart and subsequent redeployment.
- Workflow records deployment, mandatory startup migration/readiness, and smoke results; it records successful validation against the configured disposable MySQL-compatible validation target when present or a skipped validation when no configured disposable MySQL-compatible validation target is present.
- Known MySQL In-App limitations are documented for the MVP.

**Validation tests**:

- Production startup and database connectivity check.
- Authenticated persistence smoke flow.
- Restart persistence test.
- Redeployment persistence test.
- Configuration negative test in a safe environment.

**Rollback procedure**: Stop release on failed smoke verification; restore the last compatible artifact if the schema is backward compatible, otherwise use a forward fix or approved data-preserving recovery procedure. Never reset the production database as an automatic rollback.

## Directory Structure

### Documentation

```text
specs/003-azure-database-deployment/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── deployment-contract.md
├── checklists/
│   └── requirements.md
└── tasks.md                         # Existing dependency-ordered implementation work derived from the approved specification and plan
```

### Existing application files requiring modification

```text
Touching_Grass/Touching_Grass.csproj
Touching_Grass/Program.cs
Touching_Grass/appsettings.json
Touching_Grass/appsettings.Development.json
Touching_Grass/Data/ApplicationDbContext.cs
Touching_Grass/Models/ApplicationUser.cs
Touching_Grass/Models/UserPlant.cs
Touching_Grass/Repositories/ and Touching_Grass/Services/ user-data access files
tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj
tests/Touching_Grass.UnitTests/ existing and new Identity/persistence/ownership tests
.github/workflows/master_plantwisewatertracking.yml
README.md and/or feature quickstart documentation
```

### Existing migration files requiring replacement or archival treatment

```text
Touching_Grass/Data/Migrations/20261106171558_InitialIdentityAndPlants.cs
Touching_Grass/Data/Migrations/20261106171558_InitialIdentityAndPlants.Designer.cs
Touching_Grass/Data/Migrations/ApplicationDbContextModelSnapshot.cs
```

These generated SQL Server artifacts must not be used as the MySQL migration set. The implementation must follow the repository's migration review and history policy before removing, replacing, or archiving them.

### New files requiring creation

```text
Touching_Grass/Data/Migrations/20261122045502_MySqlInitial.cs
Touching_Grass/Data/Migrations/20261122045502_MySqlInitial.Designer.cs
Touching_Grass/Data/Migrations/ApplicationDbContextModelSnapshot.cs  # regenerated for MySQL
tests/Touching_Grass.UnitTests/[IMPLEMENTATION NAME TO BE CREATED: AzureIdentityPersistenceTests.cs]
tests/Touching_Grass.UnitTests/[IMPLEMENTATION NAME TO BE CREATED: AzureUserOwnershipTests.cs]
tests/Touching_Grass.UnitTests/MigrationValidationTests.cs
```

The named migration artifacts are the approved current MySQL migration set. Bracketed test names identify planned files that still need to be created during implementation; they are not unresolved placeholders.

### Configuration and workflow files requiring updates

```text
Touching_Grass/appsettings.json                 # non-secret local/default shape only
Touching_Grass/appsettings.Development.json     # local development behavior only
.github/workflows/master_plantwisewatertracking.yml
```

Production secret values belong in Azure App Service configuration or protected CI/CD secret storage, not in these files.

## Configuration and Resource Checklist

- Confirm the Azure App Service's exact site and slot names.
- Confirm Azure MySQL In-App is enabled and reachable by the App Service.
- Create/configure the protected Azure App Service `MYSQLCONNSTR_localdb` connection-string setting.
- Confirm the GitHub Actions identity has only the Azure permissions required to deploy the application and perform approved deployment verification.
- Confirm the GitHub-hosted runner does not require direct access to production MySQL In-App.
- Confirm the App Service application accesses MySQL In-App at runtime through its protected App Service database configuration.
- Confirm production migration execution is performed by the application inside Azure App Service, not by the GitHub-hosted runner.
- Do not supply production database credentials to the GitHub-hosted runner unless a separate approved design explicitly requires them.
- Confirm no secrets or raw connection strings are exposed in workflow YAML, logs, or artifacts.
- Confirm the published artifact contains no local database secret or development-only configuration.

## Constitution Check

### Pre-Design Gate

- **Code Quality and Clarity**: Pass. The plan preserves the existing .NET 10 Razor Pages architecture, requires explicit validation, and removes incompatible provider assumptions.
- **Testable Delivery**: Pass. Each phase has automated validation and release gates for Identity, persistence, ownership, migration, and deployment.
- **Consistent User Experience**: Pass. Authentication and failure behavior retain accessible, generic validation/error expectations; no unrelated UI redesign is planned.
- **Performance by Design**: Pass. The plan requires asynchronous database access, scoped queries, indexes, and a 3-second representative-flow target.
- **Maintainable Simplicity**: Pass. The free MVP persists only the approved user data and leaves read-only source JSON outside the relational scope.
- **Engineering Constraints**: Pass. ZIP values remain United States five-digit values, inputs remain untrusted, and provider-specific schema behavior is explicit.

### Post-Design Gate

- All pre-design gates remain satisfied.
- MySQL provider and migration compatibility are an explicit Phase 1 exit criterion.
- Production secrets remain outside source control and are verified by negative artifact/log checks.
- A configured disposable validation-target failure blocks publishing and deployment; when no target is configured, validation is recorded as skipped. Production startup-migration failure occurs after deployment and prevents readiness; required smoke-test failure makes the GitHub Actions workflow unsuccessful.
- No unjustified constitution violation requires complexity tracking.

## Complexity Tracking

No constitution violations are identified. The dedicated migration-validation stage and application-startup production migration, integration test fixture, and post-deployment smoke verification are required by the approved persistence and release guarantees rather than optional architectural complexity. The CI stage performs validation only.
