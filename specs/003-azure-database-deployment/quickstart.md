# Quickstart: Azure Persistent User Data Plan

This document describes how the implementation and verification should be performed after the plan is approved. It does not contain production secrets or executable implementation code.

## Local Development Prerequisites

- .NET 10 SDK and the repository's existing solution.
- No MySQL, SQLite, SQL Server, Docker, relational database, or connection string is required.
- Development Identity uses an EF Core InMemory store named `Touching_Grass.Development.Identity`.
- Development accounts and Identity state are process-local and may be lost when the application restarts.
- Oracle `MySql.EntityFrameworkCore` 10.0.12 is used only by Azure Persistence Mode.

## Configuration Contract

- Local Development does not read or require `MYSQLCONNSTR_localdb` or any connection string.
- Azure App Service supplies `MYSQLCONNSTR_localdb` through protected application configuration or connection-string settings.
- Azure startup fails safely when `MYSQLCONNSTR_localdb` is absent or invalid; it never falls back to the Development InMemory store.
- Production configuration must not be copied into `appsettings.json`, committed to GitHub, printed in logs, or embedded in published artifacts.
- Azure configuration must identify the MySQL In-App database and use the platform's required secure connection options.

## Implementation Validation Sequence

1. Confirm the selected MySQL In-App connection format and network availability from the deployed App Service.
2. Confirm Oracle provider compatibility with .NET 10/EF Core 10 for Azure Persistence Mode.
3. Regenerate the migration model for MySQL from the validated ApplicationDbContext.
4. Apply the migration to a clean disposable database.
5. Apply the same migration operation a second time and confirm it is a safe no-op.
6. Run Identity, ownership, restart-persistence, and unauthorized-access tests against an isolated database.
7. Configure the Azure App Service production connection setting without committing its value.
8. Run the GitHub Actions workflow on a feature/validation path or controlled release candidate so checkout, restore, build, tests, disposable migration validation, packaging, and deployment stages are observable; CI must not apply production migrations.
9. Confirm application startup validates configuration, applies pending migrations, and reports readiness only after migration succeeds.
10. Run the required post-deployment smoke test against `PlantwiseWaterTracking` using an isolated disposable account or safe non-destructive strategy; its failure must fail GitHub Actions.
11. Run the WCAG 2.1 AA checks and the defined 10-run post-warm-up performance measurements.
12. Record the migration version, workflow run, deployed artifact, startup/readiness and smoke result, performance results, and known MySQL In-App limitations.

## Required Verification Checks

- Register a valid user and reject invalid and duplicate registrations.
- Log in with valid credentials and reject invalid credentials.
- Log out and verify protected pages cannot be accessed.
- Save the approved MVP profile and plant/favorite data.
- In Development Mode, run repeated concurrent same-user local in-memory operations and verify: upserts do not create duplicates per source plant, favorite toggles follow parity, and read snapshots remain consistent under mutation.
- Verify a second user cannot read, change, or delete the first user's records.
- Restart the application and retrieve the original records.
- Redeploy a later artifact and retrieve the original records.
- Remove or invalidate the production configuration in a safe test environment and confirm the application does not silently use LocalDB.
- Cause a test failure and verify the GitHub Actions deployment job is blocked.
- Cause a migration validation failure and verify the application deployment job is blocked.
- Verify missing configuration, invalid credentials, unreachable database, and startup migration failure produce failed readiness and failed smoke-test status without LocalDB fallback or `EnsureCreated`.

## Operational Notes

- MySQL In-App is an MVP cost decision, not a guarantee of managed-database durability or scale.
- Production migration failures require release stop and diagnosis; do not delete the migration history or reset the database as a first response.
- Production migrations run only during application startup on the single active App Service instance; scale-out and overlapping startup migration are not approved for this MVP.
- If the MVP needs stronger backups, availability, scaling, or operational support, plan migration to a separately managed Azure database as a separate feature.
