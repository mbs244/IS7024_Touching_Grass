# Deployment Contract: Azure SQL Managed Identity Persistence

## Release Preconditions

The release MUST NOT proceed unless all are true:

- Target commit is approved for deployment.
- Dependencies restore and build succeed.
- Automated tests covering local mode and persistence behavior pass.
- Azure App Service `PlantwiseOnSQLserver` exists in resource group `Plantwise_Water_Tracking`.
- System-assigned managed identity is enabled on the target App Service.
- Azure SQL endpoint configuration references `plantwiseserver.database.windows.net` and the intended database.
- Managed identity has required least-privilege runtime + migration permissions on Azure SQL.
- Managed identity database user exists and is validated by `scripts/sql/005-verify-mi-role-membership.sql`.

## Deployment Sequence

1. Checkout approved commit.
2. Restore and build solution in Release.
3. Run automated tests.
4. Publish deployment artifact.
5. Deploy artifact to App Service `PlantwiseOnSQLserver`.
6. Ensure canonical Azure SQL configuration contract `ConnectionStrings:DefaultConnection` is present for non-Development mode.
7. App startup validates configuration, connects via managed identity, and applies pending migrations.
8. Health/readiness reports success only after successful migration and connectivity checks.
9. Measure readiness from deployment completion to first healthy response and record evidence; fail validation if time exceeds 5 minutes.
10. Run post-deployment smoke checks for registration, authentication, user-owned persistence, and access isolation.
11. Record role-membership evidence confirming least privilege and absence of `db_owner`.

## Failure Contract

- Build/test/deploy failures MUST block release completion.
- Missing/invalid Azure SQL configuration MUST fail startup in non-Development mode.
- Connectivity/authentication/migration failures MUST keep the application unready.
- Readiness timing greater than 5 minutes after deployment completion MUST fail release validation.
- Logs MAY include stage and operation names but MUST NOT include secrets or sensitive user data.
- Deployment troubleshooting MUST NOT rely on fallback to local in-memory storage in Azure.

## Configuration Contract

- Non-Development mode uses one canonical Azure SQL configuration key: `ConnectionStrings:DefaultConnection`.
- Production configuration is provided through protected Azure App Service configuration.
- SQL username/password credentials are not stored in app configuration for runtime.
- Managed identity is the required production authentication path.

## Post-Deployment Contract

The deployed app must demonstrate:

- Startup succeeds with managed identity authentication to Azure SQL.
- Pending migrations are applied safely once at startup.
- Health/readiness succeeds only when database access is operational.
- Readiness evidence proves healthy status is reached within 5 minutes after deployment completion.
- New account registration and login persist to Azure SQL.
- User plant/watering data persists across restart/redeploy.
- Cross-user data access is denied.
- Managed identity role evidence confirms broad administrative roles (including `db_owner`) are not required.
