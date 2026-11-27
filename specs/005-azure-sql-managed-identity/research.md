# Research: Azure SQL Managed Identity Persistence

## Decision 1: Use Azure SQL provider for production persistence

- **Decision**: Replace MySQL provider usage with the EF Core SQL Server provider for non-Development execution.
- **Rationale**: Target persistence platform is Azure SQL Database, and provider/runtime behavior must match that platform for migrations and Identity schema compatibility.
- **Alternatives considered**:
  - Keep MySQL provider: rejected because In-App MySQL retirement conflicts with feature scope.
  - Provider-agnostic abstraction over relational stores: rejected because it adds complexity without business value for this migration.

## Decision 2: Use App Service system-assigned managed identity for passwordless database auth

- **Decision**: Authenticate to Azure SQL from App Service via managed identity and avoid SQL username/password in production settings.
- **Rationale**: Meets security and operational goals (no stored database passwords) and aligns with Azure guidance for first-party service-to-service auth.
- **Alternatives considered**:
  - SQL authentication with secret in app settings: rejected due to explicit prohibition in requirements.
  - User-assigned managed identity: not selected because system-assigned identity is already enabled and in scope.

## Decision 3: Enforce one canonical production connection contract

- **Decision**: Use one canonical non-Development configuration contract for Azure SQL connection resolution and fail startup when missing/invalid.
- **Rationale**: Prevents ambiguous configuration paths, reduces deployment drift, and aligns with clarified requirement FR-015.
- **Operational note**: Local Development continues to ignore Azure SQL connection settings and uses in-memory persistence only.
- **Alternatives considered**:
  - Accept multiple legacy key names: rejected to avoid configuration ambiguity.
  - Auto-discover arbitrary SQL settings: rejected due to unpredictable precedence and support burden.

## Decision 4: Keep startup migration gate and readiness blocking in non-Development

- **Decision**: Continue startup migration execution before readiness and block readiness when connectivity/migration fails.
- **Rationale**: Preserves fail-safe behavior and avoids serving inconsistent authentication/data flows.
- **Alternatives considered**:
  - Start app with partially disabled data features: rejected because it complicates UX and verification.
  - Fallback to in-memory in Azure: rejected by requirements and would hide production failures.

## Decision 5: Use least-privilege managed identity permissions that still permit migrations

- **Decision**: Grant data read/write plus schema-change permissions required for startup migrations, avoiding broad admin grants.
- **Rationale**: Matches clarified permission scope while preserving automated schema initialization capability.
- **Alternatives considered**:
  - Read/write only: rejected because startup migrations can fail.
  - Broad admin rights: rejected for unnecessary privilege expansion.

## Decision 6: Regenerate active EF migration artifacts for SQL Server

- **Decision**: Replace active MySQL-specific migration artifacts with SQL Server migration artifacts for the current model.
- **Rationale**: Provider-specific migration SQL and annotations are not portable across relational providers.
- **Alternatives considered**:
  - Hand-edit MySQL migration files: rejected as high risk and hard to validate.
  - Keep old migration history untouched and create differential migration: rejected if source provider metadata remains incompatible.