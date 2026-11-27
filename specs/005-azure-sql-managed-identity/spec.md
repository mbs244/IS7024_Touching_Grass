# Feature Specification: Azure SQL Managed Identity Persistence

**Feature Branch**: `005-azure-sql-managed-identity`

**Created**: 2026-11-25

**Status**: Draft

**Input**: User description: "Replace Azure App Service In-App MySQL persistence with Azure SQL Database using Managed Identity while preserving local in-memory development workflow and existing application behaviors."

## Clarifications

### Session 2026-11-25

- Q: Which rule should decide when the app uses Azure SQL instead of the local in-memory store? → A: Environment name + required Azure SQL config check in non-development.
- Q: Which database permission scope should be granted to the App Service managed identity for production runtime? → A: Runtime read/write + migration-capable schema change permission.
- Q: Which production configuration contract should the app require for Azure SQL connectivity in non-Development mode? → A: Single required canonical config key for Azure SQL connection.
- Q: What is the canonical Azure SQL configuration key name and source? → A: `ConnectionStrings:DefaultConnection`, sourced from Azure App Service Connection String entry named `DefaultConnection`.
- Q: When Azure SQL is unavailable during startup, what behavior should the app follow before serving traffic? → A: Fail startup and stay unready until database connectivity and migrations succeed.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Keep account and saved data persistent in cloud (Priority: P1)

As an end user, I want my account and saved plant data to remain available after app restarts so I can continue where I left off.

**Why this priority**: Persistent account and user-owned data is the core business value and is required for production reliability.

**Independent Test**: Deploy to Azure, create a new account, save plant and watering data, restart the app, and verify the same user can sign in and retrieve saved records.

**Acceptance Scenarios**:

1. **Given** the app is running in Azure with an empty cloud database, **When** startup completes, **Then** required application tables exist and the app is usable.
2. **Given** a registered user saves plant and watering data, **When** the user signs out and signs back in later, **Then** the previously saved data is still available.

---

### User Story 2 - Keep local development workflow unchanged (Priority: P2)

As a developer, I want local runs to continue working without cloud resources so I can build and test quickly.

**Why this priority**: Preserving local velocity and low setup cost is essential for student contributors and rapid iteration.

**Independent Test**: Run the application locally with no cloud database configuration and verify the app starts successfully and supports the existing local workflow.

**Acceptance Scenarios**:

1. **Given** a developer runs the app locally, **When** startup executes, **Then** local in-memory behavior is used and no cloud connectivity is required.
2. **Given** local execution has no cloud credentials configured, **When** the app starts, **Then** startup still succeeds in local mode.

---

### User Story 3 - Use passwordless cloud access for the app identity (Priority: P3)

As a project maintainer, I want cloud database access to rely on app identity instead of stored passwords so operational security risk is reduced.

**Why this priority**: Passwordless access improves security posture and removes secret rotation burden while supporting long-term maintainability.

**Independent Test**: Deploy to Azure with app identity enabled, verify database operations succeed, and verify no database username/password secret is required in app configuration.

**Acceptance Scenarios**:

1. **Given** the app is deployed to Azure with managed identity enabled, **When** the app connects to the cloud database, **Then** authentication succeeds without stored database passwords.
2. **Given** password-based database authentication is not configured, **When** the app starts in Azure, **Then** the app still connects and operates using identity-based access.

---

### Edge Cases

- What happens when cloud database permissions for the app identity are missing or revoked at runtime?
- How does the system handle first startup against a newly created but empty cloud database?
- How does the system behave when cloud database connectivity is temporarily unavailable during startup?
- What happens when local development accidentally includes cloud-only configuration values?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST continue using local in-memory persistence during local development runs.
- **FR-002**: System MUST use Azure SQL Database as the persistence store when running in Azure.
- **FR-003**: System MUST authenticate to the Azure database through the App Service system-assigned managed identity.
- **FR-004**: System MUST NOT require SQL username/password authentication for production database access.
- **FR-005**: System MUST NOT require storing production database passwords in application configuration.
- **FR-006**: System MUST preserve existing user registration, sign-in, and sign-out behavior in Azure.
- **FR-007**: System MUST preserve persistence of user-owned plant and watering records in Azure.
- **FR-008**: System MUST execute pending schema migrations automatically during Azure startup before the app reports ready.
- **FR-009**: System MUST support initializing a new empty Azure database through startup migration execution.
- **FR-010**: System MUST report unhealthy status when Azure database connectivity or startup migration fails.
- **FR-011**: System MUST remove dependency on Azure App Service In-App MySQL for production operation.
- **FR-012**: System MUST keep local developer workflow unchanged so no Azure resources are required for local runs.
- **FR-013**: System MUST select local in-memory persistence only in Development mode and require Azure SQL configuration in non-Development mode, failing startup safely when required cloud configuration is missing.
- **FR-014**: System MUST run in Azure with a managed identity database user created for the App Service identity and granted only least-privilege permissions required for runtime read/write operations and startup migrations.
- **FR-015**: System MUST require exactly one canonical Azure SQL configuration key, `ConnectionStrings:DefaultConnection`, in non-Development mode and MUST fail startup safely if this key is absent or invalid.
- **FR-016**: System MUST block readiness and fail startup in non-Development mode when Azure SQL connectivity is unavailable or required migrations do not complete.
- **FR-017**: System MUST verify production functionality (startup migration, authentication, and user-owned data operations) using least-privilege managed identity permissions only.
- **FR-018**: System MUST verify that broad administrative database roles (including `db_owner`) are not required for production operation and are not part of the final permission design.

### Key Entities *(include if feature involves data)*

- **User Account**: Authenticated user identity record used for sign-in and ownership of user-specific data.
- **User Plant Record**: User-owned saved plant metadata and preference state associated with one user account.
- **Watering Data Record**: User-associated watering-related values and timestamps used by existing application workflows.
- **Application Readiness State**: Operational status indicating whether cloud persistence dependencies are available and startup completed successfully.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of validated local developer runs start successfully without requiring any Azure resources or cloud credentials.
- **SC-002**: In Azure validation runs, 100% of sampled restarts retain account and user-owned plant/watering data for test users.
- **SC-003**: In Azure validation runs, application startup completes and becomes ready within 5 minutes after deployment; readiness timing is measured from deployment completion to first healthy readiness response and captured as deployment evidence.
- **SC-004**: 0 production deployment steps require entering or storing database passwords in application settings.
- **SC-005**: 100% of production validation runs complete startup migrations and normal authenticated read/write operations using only the approved least-privilege managed identity permission set, with explicit confirmation that `db_owner` is not required.
- **SC-006**: 100% of simulated Azure SQL startup failure tests keep the application unready and prevent authenticated/data-dependent traffic until recovery.

## Assumptions

- Azure resources named in the feature request already exist and are accessible to the project subscription.
- Existing business workflows, page behavior, and domain logic remain unchanged by this persistence modernization.
- Legacy MySQL data migration is intentionally excluded, and production starts from a new or independently prepared database state.
- The App Service managed identity can be granted the required database permissions before production traffic validation.