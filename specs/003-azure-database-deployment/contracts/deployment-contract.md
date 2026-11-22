# Deployment Contract: Azure Persistent User Data

## Release Preconditions

The release MUST NOT proceed unless all are true:

- The intended commit is merged into `master` or is being run through an explicitly authorized release validation.
- Dependencies restore successfully.
- The application builds successfully.
- The complete automated test suite passes.
- When a disposable MySQL-compatible validation target is configured, migration validation passes against it; otherwise, external validation is recorded as skipped. Azure startup migration, readiness, and smoke verification remain mandatory.
- The protected production connection configuration is available to the deployed App Service without being printed.
- The MySQL migration model validates against a clean disposable database when available; production migration execution is reserved for application startup.

## Deployment Sequence

1. Checkout the release commit.
2. Install the supported .NET 10 SDK.
3. Restore dependencies.
4. Build in Release configuration.
5. Run unit and required persistence/ownership tests.
6. Validate the MySQL migration against an isolated database when available.
7. Publish the application artifact.
8. Deploy the artifact to the `PlantwiseWaterTracking` production App Service slot.
9. Application startup validates production configuration and applies pending migrations through Oracle MySql.EntityFrameworkCore.
10. The application reports readiness only after startup migration succeeds.
11. Execute the required post-deployment startup, connectivity, authentication, persistence, logout, and cross-user-denial smoke verification.

## Failure Contract

- A failure in steps 1–6 MUST prevent publishing or Azure application deployment.
- A failed deployment, startup migration, readiness check, or smoke verification MUST make the workflow fail visibly.
- Logs MAY identify the failed stage and migration name but MUST NOT include credentials, full connection strings, passwords, hashes, tokens, SQL secrets, or personal data.
- A migration failure MUST leave the release in a diagnosable state; the workflow MUST NOT reset or drop the production database.
- The application MUST NOT use `EnsureCreated` and MUST NOT automatically roll back production migrations. Application rollback is permitted only when the earlier artifact is compatible with the applied schema.
- The MVP MUST operate with one active App Service instance; overlapping startup migration is not approved, provider-supported locking is used when available, and scale-out requires architectural review.

## Configuration Contract

- The application resolves the production database through the approved `DefaultConnection` configuration contract.
- Azure App Service protected configuration is the source of the production connection value.
- No production secret is committed to YAML, JSON, source code, or published artifacts.
- Missing configuration is a hard failure in production and MUST NOT fall back to LocalDB.

## Post-Deployment Contract

The deployed endpoint MUST demonstrate:

- Application startup without database provider errors.
- Database connectivity.
- Registration without email verification.
- Login and logout.
- Save and retrieval of account profile, default ZIP, and plant/favorite data.
- Denial of a cross-user record request.
- Persistence after an application restart or equivalent controlled restart test.
- No sensitive data is retained in smoke-test logs or uploaded artifacts; disposable test data is cleaned up or isolated.
