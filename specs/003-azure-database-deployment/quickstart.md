# Quickstart: Azure Persistent User Data Plan

This document describes how the implementation and verification should be performed after the plan is approved. It does not contain production secrets or executable implementation code.

## Local Development Prerequisites

- .NET 10 SDK and the repository's existing solution.
- A disposable MySQL-compatible development database or approved local MySQL environment; do not use the Azure production database for local development.
- Oracle `MySql.EntityFrameworkCore` 10.0.12 with the compatible EF Core 10.0.x package family validated for the application.
- A local connection string supplied through user secrets or an environment-specific configuration source.
- An isolated test database for persistence and migration tests.

## Configuration Contract

- Local development uses a non-production connection setting named `ConnectionStrings:DefaultConnection`.
- Azure App Service supplies the production value through protected application configuration or connection-string settings.
- The application must resolve the same logical configuration name in both environments.
- Production configuration must not be copied into `appsettings.json`, committed to GitHub, printed in logs, or embedded in published artifacts.
- Azure configuration must identify the MySQL In-App database and use the platform's required secure connection options.

## Implementation Validation Sequence

1. Confirm the selected MySQL In-App connection format and network availability from the deployed App Service.
2. Replace the SQL Server EF Core provider only after confirming Oracle provider compatibility with .NET 10/EF Core 10.
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
