# Data Model: Azure SQL Managed Identity Persistence

## Scope

Azure SQL Database is the authoritative store for ASP.NET Core Identity records and approved user-owned persistence in non-Development mode. Development mode remains process-local and in-memory only.

## Entities

### ApplicationUser

- **Purpose**: Authenticated account identity and owner of user-specific records.
- **Identity fields**: Framework-managed user identifier, normalized identity fields, security metadata.
- **Profile fields**: Default ZIP code and existing profile properties.
- **Constraints**:
  - User uniqueness remains enforced by Identity indexes.
  - Default ZIP code remains required and validated by existing application rules.

### UserPlant

- **Purpose**: User-owned saved plant/favorite state with timestamps.
- **Fields**:
  - Record identifier
  - Required owner identifier (`ApplicationUser` FK)
  - Stable source plant identifier
  - Name snapshot fields
  - Favorite flag
  - Created/updated timestamps
- **Constraints**:
  - One owner per record
  - Ownership resolved server-side from authenticated principal
  - Per-user uniqueness for saved plant identity remains enforced

### Identity Supporting Entities

- **Purpose**: Framework-managed entities for logins, claims, roles, tokens, and security state.
- **Constraint**: Provider-generated schema must remain compatible with ASP.NET Core Identity behavior in .NET 10.

### Migration History

- **Purpose**: Tracks applied SQL Server EF Core migrations.
- **Constraints**:
  - Managed exclusively by EF Core migration pipeline
  - Required for safe startup migration idempotency
  - Must not be dropped/reset during normal deployment troubleshooting

## Relationships

```text
ApplicationUser 1 ──────── * UserPlant
ApplicationUser 1 ──────── * Identity security/login entities
UserPlant * ─────────────── 1 Source plant reference snapshot
```

## Ownership Rules

1. User ownership for `UserPlant` writes is derived from authenticated server identity.
2. User-owned reads/writes must include owner predicate filtering.
3. Client-supplied owner identifiers are ignored for authorization decisions.
4. Cross-user access attempts return unauthorized/not-found behavior without data disclosure.

## Migration Boundaries

- Active migrations for this feature must target SQL Server provider semantics for Azure SQL.
- MySQL-specific migration artifacts are not the runtime migration source for this feature.
- Startup migration runs only in non-Development mode before readiness.
- Development mode does not execute relational migrations.