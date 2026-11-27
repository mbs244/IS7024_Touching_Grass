# Copilot Instructions

## Project Guidelines
- PlantWise is expanding from California-only support to all United States ZIP codes; ZIP validation and documentation must not restrict users to California.

## Architecture Decisions
- For feature 005-azure-sql-managed-identity, Visual Studio/local development uses existing JSON-backed and in-memory services and does not require Azure resources, SQL Server, Docker, or any relational database.
- Local authentication and user-data persistence are not required; startup database migrations do not execute locally.
- Azure App Service uses `ConnectionStrings:DefaultConnection` with managed identity authentication; Azure authentication and user-data persistence are required; startup migrations run before readiness.
- Missing `ConnectionStrings:DefaultConnection` causes readiness failure.
- Future planning artifacts must reflect Azure SQL persistence, least-privilege managed identity permissions, and startup-readiness validation covering both local and Azure modes.
