# Implementation Plan: Azure SQL Managed Identity Persistence

**Branch**: `005-azure-sql-managed-identity` | **Date**: 2026-11-25 | **Spec**: [spec.md](./spec.md)

**Input**: Approved feature specification for replacing Azure App Service In-App MySQL persistence with Azure SQL Database using system-assigned managed identity while preserving local in-memory development behavior.

## Summary

The implementation will keep current local-development behavior unchanged (`Development` mode uses EF Core InMemory and no Azure dependencies) while replacing the Azure persistence path with Azure SQL Database. In non-Development mode, startup will require the canonical key `ConnectionStrings:DefaultConnection` (from App Service Connection String name `DefaultConnection`), authenticate with system-assigned managed identity, apply pending EF Core migrations, and block readiness on connectivity or migration failure.

This change removes MySQL package/runtime coupling, replaces MySQL-specific migration artifacts with SQL Server migration artifacts, and updates deployment validation so the App Service `PlantwiseOnSQLserver` in resource group `Plantwise_Water_Tracking` can run against `plantwiseserver.database.windows.net` without database passwords.

## Technical Context

**Language/Version**: C# / .NET 10 (Razor Pages application + xUnit tests)

**Primary Dependencies**: ASP.NET Core Identity, Entity Framework Core 10.x, SQL Server EF provider, Microsoft.Data.SqlClient, Azure App Service managed identity, GitHub Actions deployment workflow

**Storage**: Azure SQL Database (`free-sql-db-1820520`) for non-Development; EF Core InMemory store for Development

**Testing**: Existing unit tests plus targeted integration and deployment checks for startup migration, readiness, authentication, and persistence

**Target Platform**: Azure App Service `PlantwiseOnSQLserver` (system-assigned managed identity enabled) in `Plantwise_Water_Tracking`

**Project Type**: Single ASP.NET Core Razor Pages web application with a separate test project

**Performance Goals**: Preserve current user-facing responsiveness and ensure startup-to-ready remains within 5 minutes with migration on empty/non-empty database

**Constraints**: Local workflow unchanged; no SQL authentication; no database password storage; fail-safe startup in non-Development when `ConnectionStrings:DefaultConnection` is missing/invalid; production permission model must prove least privilege without `db_owner`

**Scale/Scope**: Classroom MVP scale, single active App Service instance, Identity + user-owned plant/watering persistence only

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Code Quality and Clarity**: PASS — plan keeps environment-based composition in `Program.cs` and provider changes isolated to persistence and deployment boundaries.
- **Testable Delivery**: PASS — plan requires updating/adding automated tests for local mode, Azure mode startup migration behavior, and persistence ownership flows.
- **Consistent User Experience**: PASS — no user-facing workflow redesign; authentication and data flows remain behaviorally consistent.
- **Performance by Design**: PASS — migration and health/readiness checks are explicit; no added per-request external calls beyond required database usage.
- **Maintainable Simplicity**: PASS — removes outdated MySQL path and keeps one production relational provider.

Post-design re-check: PASS. No constitutional violations require exception handling.

## Project Structure

### Documentation (this feature)

```text
specs/005-azure-sql-managed-identity/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── deployment-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
Touching_Grass/
├── Data/
├── Models/
├── Pages/
├── Repositories/
├── Services/
├── Properties/
└── Program.cs

tests/
└── Touching_Grass.UnitTests/
```

**Structure Decision**: Keep the existing single Razor Pages application plus one test project. Modify only persistence composition, EF provider/migrations, deployment workflow/configuration references, and supporting docs.

## Validation & Evidence Strategy

- Define one authoritative runtime configuration contract: `ConnectionStrings:DefaultConnection`.
- Create and verify a managed identity database user for App Service with least-privilege runtime + migration permissions.
- Validate startup readiness timing with explicit measurement from deployment completion to first healthy readiness response; pass threshold is <= 5 minutes.
- Capture evidence for each deployment validation run: timing result, migration success, role membership snapshot, and functional smoke results.
- Include negative validation proving broad administrative roles (including `db_owner`) are not required for normal production operation.

## Complexity Tracking

No constitution exceptions identified.
