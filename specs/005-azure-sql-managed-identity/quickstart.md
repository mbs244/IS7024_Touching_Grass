# Quickstart: Azure SQL Managed Identity Persistence Plan

This quickstart describes implementation validation flow after planning approval. It excludes production secrets and executable credentials.

## Local Development Prerequisites

- .NET 10 SDK and existing repository solution.
- No Azure resources required for local development.
- Development mode continues to use in-memory persistence behavior.

## Azure Prerequisites

- Resource group: `Plantwise_Water_Tracking`
- App Service: `PlantwiseOnSQLserver`
- System-assigned managed identity: enabled
- Azure SQL server: `plantwiseserver.database.windows.net`
- Azure SQL database: `free-sql-db-1820520`

## Configuration Contract

- Development mode does not require Azure SQL configuration.
- Non-Development mode requires one canonical Azure SQL configuration key: `ConnectionStrings:DefaultConnection`.
- In Azure App Service, this value is supplied through the Connection Strings entry named `DefaultConnection`.
- Production startup fails safely if the canonical key is missing/invalid.
- Production runtime authentication uses managed identity, not SQL username/password.

## Validation Sequence

1. Verify local execution succeeds without Azure SQL settings.
2. Confirm SQL Server EF provider and package set are aligned to .NET 10.
3. Regenerate and validate active SQL Server migrations from current `ApplicationDbContext`.
4. Execute `scripts/sql/005-create-mi-user-least-privilege.sql` to create the managed identity database user and grant required least-privilege roles.
5. Execute `scripts/sql/005-verify-mi-role-membership.sql` to verify required roles and confirm `db_owner` is absent.
6. Configure canonical Azure SQL connection contract in App Service (without credential secrets).
7. Deploy application artifact to `PlantwiseOnSQLserver`.
8. Run `scripts/measure-startup-readiness.ps1` and capture readiness timing evidence from deployment completion to first healthy `/health` response.
9. Verify startup migration runs and readiness remains blocked until successful.
10. Execute post-deployment smoke tests for registration, login, persistence, ownership, and restart/redeploy persistence.
11. Record validation evidence: role-membership verification output, readiness timing output, migration success, and smoke test result.

## Required Verification Checks

- Local development startup remains unchanged and succeeds without cloud dependencies.
- Azure startup succeeds with managed identity and no SQL password.
- Empty database initialization succeeds via startup migration.
- Existing authenticated flows and user-owned persistence work in Azure.
- Missing configuration, missing permission, and transient unavailability produce unready/fail-safe behavior.
- Startup readiness pass threshold: first healthy readiness response occurs within 5 minutes after deployment completion.
- Negative permission validation proves broad administrative roles (including `db_owner`) are not required.

## Operational Notes

- This feature explicitly does not migrate legacy MySQL data.
- Startup migration remains a single-instance MVP assumption and must be revisited before scale-out.
- Production incidents should preserve migration history for diagnosis rather than resetting schema state.