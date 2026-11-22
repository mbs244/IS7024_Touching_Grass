# Deployment verification evidence

This directory is reserved for sanitized release evidence only. Do not commit credentials,
connection strings, cookies, passwords, personal data, raw provider logs, or stack traces.

## Local verification completed

- Oracle `MySql.EntityFrameworkCore` 10.0.12 restored for `net10.0`.
- EF Core generated an idempotent MySQL script containing Identity tables, `UserPlants`, and
  `__EFMigrationsHistory`.
- The application and xUnit project build successfully; all 10 tests pass.
- No disposable MySQL server was available in this environment, so clean-database apply and
  repeated apply were not executed locally.

## Azure verification pending

The following require access to the configured `PlantwiseWaterTracking` App Service and its
protected MySQL In-App setting: startup migration, readiness, smoke test, controlled restart,
redeployment persistence, WCAG browser checks, and 10-run production-like performance checks.
The workflow fails the required smoke-test step unless its protected `SMOKE_TEST_*` settings are
configured. Azure MySQL In-App runtime connectivity and server feature compatibility are not
claimed until those checks are run.
