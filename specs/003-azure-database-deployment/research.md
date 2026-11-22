# Research: Azure Persistent User Data

## Decision 1: Use Azure MySQL In-App for the free MVP

- **Decision**: Target the enabled Azure MySQL In-App database for the initial MVP.
- **Rationale**: This is the clarified product decision and minimizes cloud cost while validating authentication and user-owned persistence.
- **Compatibility consequence**: The current SQL Server provider, SQL Server-specific migrations, SQL Server connection string, and `UseSqlServer` registration cannot be reused as the MySQL production configuration. The provider, connection string, generated migration SQL, and runtime verification must all target MySQL.
- **Operational limitation**: MySQL In-App is treated as an MVP platform with documented durability, backup, scaling, availability, and operational limitations. A separately managed Azure database remains the follow-up path if the application outgrows the free MVP.
- **Alternatives considered**:
  - Azure Database for MySQL Flexible Server: stronger production durability and operations, but not selected because the MVP must remain free.
  - Azure SQL Database: closer to the current SQL Server implementation, but not selected because the approved MVP platform is MySQL In-App.

## Decision 2: Use Oracle MySql.EntityFrameworkCore 10.0.12

- **Decision**: Use Oracle's `MySql.EntityFrameworkCore` package version `10.0.12` with the project's `net10.0` target and EF Core `10.0.0` package family. The package metadata publishes a `net10.0` dependency group requiring `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Relational` `10.0.12`; the project must align its Microsoft EF Core packages to that compatible 10.0.x provider dependency during implementation without changing the target framework.
- **Comparison**: The latest Pomelo package verified from the NuGet package index was `Pomelo.EntityFrameworkCore.MySql` `9.0.0`, whose package metadata targets `net8.0` and requires EF Core Relational `[9.0.0, 9.0.999]`. No Pomelo EF Core 10 package was verified, so Pomelo is not selected.
- **Compatibility boundary**: NuGet metadata verifies the Oracle provider's .NET 10 and EF Core 10 package compatibility. The exact Azure MySQL In-App server version and provider-supported feature subset must be confirmed against the provisioned service during T001/T006; no server version is invented here.
- **Required capabilities**: The provider must be validated for ASP.NET Core Identity tables, EF Core migration generation and `dotnet ef database update`, MySQL TLS settings, and the Azure connection-string format. A clean disposable MySQL-compatible database must migrate from zero to the latest model, and a second migration attempt must be a no-op.
- **TLS and connection settings**: Use the provider's documented MySQL connection-string options, encrypted transport, certificate validation appropriate to Azure, bounded timeout, and pooling. Keep the value in App Service protected configuration and never log it.
- **Known limitations**: Provider-specific SQL, type mappings, index limits, Identity length constraints, migration locking behavior, and Azure MySQL In-App server capabilities require integration validation. Startup migration is approved only for the one-instance MVP and must be reconsidered before scale-out.
- **Alternatives considered**:
  - Keep SQL Server: rejected because it conflicts with the approved MySQL platform.
  - Use Pomelo: not selected because the verified package targets EF Core 9, not this project's EF Core 10 target.
  - Use provider-agnostic SQL only: rejected because Identity and migration lifecycle still require a provider-aware EF Core implementation.

## Decision 3: Regenerate migrations for MySQL and isolate SQL Server history

- **Decision**: Treat the existing SQL Server migration set in `Touching_Grass/Data/Migrations/` as historical, preserve it in Git history, and move it out of the active migrations directory before generating a clean MySQL-compatible migration set. The implementation task must use either a documented archive directory outside the runtime migration assembly or a separate provider-specific history structure; the active runtime set must contain only Oracle MySQL migrations.
- **Rationale**: Existing migrations contain SQL Server-specific column types and annotations. Editing those generated files manually would make the migration history difficult to trust and could leave an invalid model snapshot.
- **Data safety**: Before replacing migrations in any environment containing data, export or preserve required data and document the migration boundary. The MVP's first production database should be initialized with the MySQL migration; no destructive migration is permitted against existing user data. The generated MySQL migration must include all ASP.NET Core Identity tables, application tables, constraints, indexes, and normal EF migration history.
- **Operational safety**: SQL Server migration classes must not remain discoverable as the active runtime migration set, must not be executed against MySQL, and must not be replaced with `EnsureCreated`. Application startup applies only pending MySQL migrations; repeated startup is idempotent and does not automatically roll back production migrations.
- **Alternatives considered**:
  - Modify SQL Server migration files in place: rejected because generated provider-specific operations should not be hand-translated without provider validation.
  - Run SQL Server migrations against MySQL: rejected as incompatible.

## Decision 4: Store production connection settings outside source control

- **Decision**: Provide the production connection string through Azure App Service protected configuration, using an application configuration name that the application resolves consistently as `ConnectionStrings:DefaultConnection`.
- **Rationale**: Azure owns the production secret, local configuration remains independent, and repository history and published artifacts do not contain credentials.
- **Validation**: Deploy with the setting present, verify database connectivity, remove or invalidate it in a non-production slot/test environment, and verify the app fails safely without falling back to LocalDB.
- **Alternatives considered**:
  - Commit the production string to `appsettings.json`: rejected because it exposes credentials.
  - Store secrets in the workflow YAML: rejected because workflow source is not an appropriate secret store.

## Decision 5: Validate migrations in CI and apply production migrations at startup

- **Decision**: GitHub Actions validates the MySQL migration model against a disposable MySQL-compatible database when available, but never applies migrations to MySQL In-App. After deployment, the application validates protected production configuration and applies only pending EF Core migrations during startup before readiness or normal authenticated/data-dependent traffic.
- **Rationale**: An external GitHub-hosted runner must not be required to connect to MySQL In-App for production migration execution. Startup migration keeps execution inside the App Service network/runtime and makes readiness and required smoke verification the release gate.
- **Operational requirement**: The MVP has one active App Service instance; scale-out and overlapping startup migration are not approved. Use provider-supported migration locking when available, keep migration execution idempotent, preserve EF migration history, do not use `EnsureCreated`, and do not automatically roll back production migrations.
- **Failure behavior**: Missing configuration, invalid credentials, unreachable database, or migration failure prevents readiness and causes the required smoke test and GitHub Actions workflow to fail. User output remains generic and logs are actionable but redacted.
- **Alternatives considered**:
  - Apply production migrations from GitHub Actions: rejected because external runner connectivity to MySQL In-App is not an approved architecture.
  - Apply migrations manually: rejected because it is not repeatable or verifiable in the required release path.

## Decision 6: Preserve the existing Identity model and add only approved MVP persistence

- **Decision**: Keep ASP.NET Core Identity as the authentication boundary and validate its tables against the MySQL provider. Persist account profile data, default ZIP code, saved plants/favorites, and audit timestamps with required user ownership.
- **Rationale**: This follows the existing architecture and clarified data scope without adding email delivery, password reset, social login, MFA, roles, or calculation history.
- **Validation**: Verify registration, login, logout, unique email behavior, password rules, ownership filtering, cross-user denial, and persistence after restart/redeployment.

## Decision 7: Validate deployment with non-destructive smoke checks

- **Decision**: Use automated unit/integration verification before deployment and a post-deployment smoke check that confirms startup, database reachability, registration/login, save, logout, login again, and retrieval.
- **Rationale**: A successful package upload alone does not prove that the production provider, connection string, schema, Identity store, and ownership rules work together.
- **Cost constraint**: Keep the checks small and non-destructive; use isolated test data or a dedicated verification account and clean it up where the platform allows.
