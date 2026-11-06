# Research: Authenticated User Accounts and Personal Plants

## Decision 1: Use the platform's established secure web-account model

- **Decision**: Use ASP.NET Core Identity for local email/password authentication with the Identity Entity Framework store and cookie-session behavior. This feature creates no formal Identity roles.
- **Rationale**: The feature requires password protection, sign-in/sign-out, protected Razor Pages, account isolation, and generic authentication failures. A mature platform identity model addresses these concerns without inventing credential storage or session handling.
- **Alternatives considered**: A custom password and session implementation was rejected because it increases security risk and duplicates established authentication behavior. External social sign-in is explicitly out of scope.

## Decision 2: Persist account and personal-plant data transactionally

- **Decision**: Use Entity Framework Core with SQL Server LocalDB for local development and the Identity Entity Framework store. Account credentials, default ZIPs, personal plants, and favorite state are relational data, never JSON files.
- **Rationale**: Account identity, one default ZIP, personal plant associations, and favorite state require durable, user-scoped relationships and uniqueness enforcement. Transactional persistence supports duplicate prevention and safe updates.
- **Alternatives considered**: JSON files were rejected for mutable multi-user data because concurrent writes, credential safety, ownership isolation, and uniqueness would be unreliable. In-memory storage was rejected because it cannot meet cross-session persistence.

## Decision 3: Keep source datasets and recommendation calculation authoritative

- **Decision**: Reuse the existing ZIP lookup, plant repository, deterministic mock-weather inputs, and `WateringCalculatorService` for saved-plant recommendations.
- **Rationale**: The specification explicitly preserves current behavior, prohibits a second calculation formula, and excludes external weather APIs from this feature. Saved plants store source identity snapshots while recommendations are recalculated when opened.
- **Alternatives considered**: Persisting recommendation snapshots was rejected because weather and default ZIP values change. A separate personal calculation path was rejected because it would risk formula drift.

## Decision 4: Enforce ownership at every personal operation

- **Decision**: Treat the authenticated user identity as the sole authority for account settings, personal plant reads/writes, and favorite actions; unauthorized or cross-user requests fail safely.
- **Rationale**: URL or form identifiers are untrusted and must never determine ownership. This supports the privacy requirements and allows tests to verify both signed-out and cross-user denial.
- **Alternatives considered**: Hiding links without server-side ownership checks was rejected because it does not protect direct requests.

## Decision 5: Keep the current anonymous lookup flow intact

- **Decision**: Add personalization around the existing search flow without requiring authentication for the existing visitor journey, unless the current application access rules are later changed explicitly.
- **Rationale**: The feature requires the current plant search and recommendation behavior to remain available and non-regressing. A signed-in successful search additionally upserts the user's personal plant entry.
- **Alternatives considered**: Making all searches require authentication was rejected because it changes existing access behavior and conflicts with the preservation requirement.

## Resolved planning assumptions

- Registration and sign-in use email address plus password.
- Duplicate registration returns a generic already-registered response.
- One user has one current default five-digit ZIP code.
- Personal plant uniqueness is one entry per user and source Plant identifier.
- Favorite ordering is favorites first, followed by a stable deterministic order.
- Saved-plant recommendations are current calculations, not permanent snapshots.

## Remaining implementation-level decisions

- Exact password strength values, lockout values, cookie lifetime, and deployment-specific connection settings should follow the platform security baseline and project environment configuration during implementation.
- This feature uses the current deterministic mock-weather inputs only. Open-Meteo, Visual Crossing, and other external weather providers remain separate future work.

## Security and measurement decisions

- Password storage and verification are delegated entirely to ASP.NET Core Identity; custom hashing and readable password storage are prohibited.
- Logging review must cover new log statements, exception handlers, serialization, authentication failures, and all credential-related fields. Safe user messages must exclude connection strings, stack traces, database details, credentials, and unrelated personal data.
- SC-001 and SC-002 use the defined local LocalDB/Release/10-iteration/warm-up/request-boundary measurement conditions in `spec.md`, reporting median and slowest execution without startup, migration, or debugger overhead.
