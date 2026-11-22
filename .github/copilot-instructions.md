# Copilot Instructions

## Project Guidelines
- PlantWise is expanding from California-only support to all United States ZIP codes; ZIP validation and documentation must not restrict users to California.

## Architecture Decisions
- For feature 003-azure-database-deployment, Visual Studio/local development uses existing JSON-backed and in-memory services and does not require MySQL, SQLite, SQL Server, Docker, or any relational database.
- Local authentication and user-data persistence are not required; startup database migrations do not execute locally.
- Azure App Service uses MYSQLCONNSTR_localdb; Azure authentication and user-data persistence are required; startup migrations run before readiness.
- Missing MYSQLCONNSTR_localdb causes readiness failure.
- Future planning artifacts must reflect Azure-only persistence and migrations, with validation covering both local and Azure modes.