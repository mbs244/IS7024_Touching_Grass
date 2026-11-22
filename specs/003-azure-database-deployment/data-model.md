# Data Model: Azure Persistent User Data

## Scope

The relational database is authoritative for Identity accounts and the approved MVP user-owned data. The existing read-only plant and ZIP source datasets may remain outside this relational model unless separately expanded; saved user selections reference source data through stable identifiers and snapshots.

## Entities

### ApplicationUser

- **Purpose**: Registered account and authenticated owner of application data.
- **Identity fields**: Framework-managed user identifier, normalized email, login/security metadata.
- **Profile fields**: Default ZIP code, required for the MVP profile flow.
- **Constraints**:
  - Normalized email is unique.
  - Default ZIP code follows the application's five-digit United States ZIP validation rule.
  - Passwords are never stored as clear text; Identity-managed hashes and security metadata are persisted.

### UserPlant

- **Purpose**: A plant saved by a user, including favorite state and audit timestamps.
- **Fields**:
  - Framework-generated record identifier.
  - Required application user identifier.
  - Required stable source plant identifier.
  - Common-name snapshot.
  - Scientific-name snapshot when available.
  - Favorite flag.
  - Created timestamp.
  - Last-searched or last-updated timestamp as defined by the approved application behavior.
- **Constraints**:
  - Must reference exactly one ApplicationUser.
  - User ownership is immutable from client requests.
  - A user cannot create duplicate records for the same source plant where the feature's uniqueness rule applies.
  - User queries must filter by the current authenticated user before applying record identifiers.

### Source Plant

- **Purpose**: Read-only source record used to resolve plant selections.
- **Storage boundary**: Existing source dataset may remain JSON for this MVP.
- **Relationship**: UserPlant stores the stable source identifier and display snapshots so a saved record remains understandable if source display names change.

### Source ZIP Location

- **Purpose**: Read-only source record used to validate and resolve ZIP selections.
- **Storage boundary**: Existing source dataset may remain JSON for this MVP.
- **Relationship**: ApplicationUser stores the user's selected default ZIP value; the source dataset resolves coordinates and location data at runtime.

### Migration History

- **Purpose**: Records which versioned database schema changes have been applied.
- **Constraints**:
  - Must be created and maintained by the selected MySQL-compatible EF Core provider.
  - Must be checked before declaring deployment successful.
  - Must never be deleted or reset as part of ordinary deployment.

## Relationships

```text
ApplicationUser 1 ──────── * UserPlant
ApplicationUser 1 ──────── 1 default ZIP value
UserPlant * ─────────────── 1 Source Plant snapshot/reference
ApplicationUser 1 ──────── * Identity security and login records
```

## Ownership Rules

1. Every UserPlant create operation obtains the owner from the authenticated server-side identity.
2. Every UserPlant read, update, and delete operation includes the authenticated owner's identifier in its data-access predicate.
3. A client-supplied user identifier is ignored for ownership decisions.
4. A cross-user record request returns an indistinguishable unavailable/forbidden result without revealing the target record.
5. Account deletion behavior must follow the existing Identity relationship policy and must not leave unauthorized orphaned user records.

## Migration Boundaries

- Existing SQL Server migration artifacts are not portable to MySQL and are not the production migration source; preserve them in Git history but move them outside the active runtime migration directory.
- The active migration model must use Oracle `MySql.EntityFrameworkCore` 10.0.12 with the compatible EF Core 10.0.x package family and include Identity tables, ApplicationUser profile fields, UserPlant relationships, unique constraints, indexes, and migration history.
- GitHub Actions validates the migration model against a disposable database when available; the deployed application applies pending production migrations during startup before readiness. `EnsureCreated` and automatic production migration rollback are prohibited.
- The initial MVP production database must be initialized without destructive data replacement.
- Later schema changes require a new reviewed migration and a compatibility check against existing user accounts and UserPlant records.
