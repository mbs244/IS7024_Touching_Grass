# Feature Specification: Azure Persistent User Data

**Feature Branch**: `user_data_sheppamu`
**Created**: 2026-11-21
**Status**: Draft
**Input**: User description: Replace the local-development-only database configuration with a production-ready Azure deployment strategy supporting authentication, user-owned data, persistent storage, database migrations, CI/CD verification, and secure production connection-string management for Plantwise Water Tracking.

## Feature Summary

Plantwise Water Tracking will use the Azure-hosted application database as the authoritative persistent store for account records and user-owned application data after deployment. Users will be able to register, authenticate, sign out, save private data, and retrieve that data after application restarts and redeployments. The existing GitHub Actions workflow will remain the deployment path and must verify the application before releasing it to the `PlantwiseWaterTracking` Azure App Service. During Visual Studio/local development, the application will continue using its existing JSON-backed and in-memory services and will not require a relational database.

The production database configuration must be distinct from local-development configuration. Production credentials and connection strings must be supplied by Azure configuration or an equivalent protected secret mechanism, never committed to source control. Database schema changes must be versioned, repeatable, observable, and applied safely as part of the production release process.

## Business Objectives

- Allow a new user to create an account and securely sign in.
- Keep each user's saved application data private and associated with that user.
- Preserve user accounts and user-owned data across application restarts and successful redeployments.
- Limit the MVP to one active Azure App Service instance. Horizontal scale-out, concurrent App Service instances, and overlapping startup migration are deferred; the architecture MUST be reevaluated before increasing the instance count above one.
- Support multiple users within the single application instance; concurrent requests from different users MUST preserve user ownership and data isolation.
- Make the deployed application operational against the Azure-hosted database without manual code or secret changes after release.
- Prevent a failed build, failed test run, invalid configuration, or failed migration from being presented as a successful production deployment.
- Minimize cloud cost while retaining persistent storage and a supportable production configuration.

## Clarifications

### Session 2026-11-21

- Q: Which Azure database should be the production persistence store for Plantwise Water Tracking? → A: Azure MySQL In-App Database for the free minimum viable product.
- Q: How should database migrations be applied during deployment? → A: GitHub Actions validates migrations only; the deployed application applies pending migrations during startup before readiness and normal authenticated/data-dependent traffic.
- Q: Which user-owned data must the MVP persist in the Azure database? → A: Account profile, default ZIP code, saved plants/favorites, and timestamps.
- Q: Should new users be required to verify their email address before they can log in? → A: No email verification; users can log in after registration for the MVP.
- Q: Should the MVP include a password-reset flow for users who forget their password? → A: Password reset is out of scope for the MVP.

### Session 2026-11-22 — Revised environment architecture

- Q: What persistence is required during Visual Studio/local development? → A: None. Local execution uses the existing JSON-backed and in-memory services and does not require MySQL, SQLite, SQL Server, Docker, or another relational database.
- Q: What persistence is required in Azure? → A: ASP.NET Core Identity and user-owned data must persist through `MYSQLCONNSTR_localdb`.
- Q: When do database migrations execute? → A: Only in Azure App Service startup, before readiness. Local startup does not execute database migrations.

## Scope

### In Scope

- ASP.NET Core Identity registration, login, logout, and authenticated sessions.
- Persistent storage for user accounts and user-owned application data.
- Entity Framework-compatible database schema and versioned migration behavior.
- Azure-only persistent storage for Identity accounts and user-owned data.
- Azure App Service production configuration and protected connection-string management.
- Data ownership relationships and server-side authorization for saved records.
- GitHub Actions verification and deployment readiness for the `master` release path.
- Verification of persistence after application restart and redeployment.
- Local-mode validation that the application runs without a relational database.

### Out of Scope

- Social login providers.
- Email verification and password reset.
- Multi-factor authentication.
- Administrative dashboards.
- Role-based authorization.
- Data analytics.
- Data import/export.
- UI redesign unrelated to authentication or user data.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and access an account (Priority: P1)

As a user, I want to register an account and log in so that my application data can be associated with me and protected from other users.

**Why this priority**: Authentication is the prerequisite for private user-owned data and is the primary production value of this feature.

**Independent Test**: In Azure persistence mode, register a valid new account, sign in with its credentials, verify an authenticated page is accessible, sign out, and verify the authenticated page is no longer accessible. Development Mode may retain existing non-persistent behavior.

**Acceptance Scenarios**:

1. **Given** a visitor has a unique valid email address and a password that satisfies the published password rules, **When** the visitor submits registration, **Then** an account is created, no password or password hash is shown to the user, and the user receives a clear success or next-step message.
2. **Given** a registered user provides valid credentials, **When** the user submits login, **Then** the user is authenticated and can access pages requiring authentication.
3. **Given** an authenticated user, **When** the user selects logout, **Then** the session is ended and subsequent requests cannot access authenticated-only pages without logging in again.
4. **Given** an account already exists for an email address, **When** another registration is attempted with that email address, **Then** registration is rejected without creating a duplicate account.

---

### User Story 2 - Save and retrieve private data (Priority: P1)

As a signed-in user, I want to save application data and retrieve it later so that my work is not lost and is not visible to other users.

**Why this priority**: Persistent, user-owned data is the business outcome that distinguishes a production application from a local-only demonstration.

**Independent Test**: In Azure persistence mode, create two separate accounts, save distinct records for each account, retrieve each account's records, and verify that neither account can read, modify, or delete the other account's records. Development Mode does not require persistent user-data storage.

**Acceptance Scenarios**:

1. **Given** an authenticated user submits valid data, **When** the save operation succeeds, **Then** the record is stored with that user's ownership and is available on the user's next authenticated visit.
2. **Given** a user has previously saved data, **When** the user signs out, signs back in, and opens the relevant page, **Then** the previously saved data is displayed for that user.
3. **Given** two users have saved records, **When** either user requests their saved data, **Then** only records owned by the authenticated user are returned.
4. **Given** a user attempts to access another user's record by changing an identifier or request parameter, **When** the request is processed, **Then** access is denied or the record is treated as unavailable without disclosing its existence or contents.

---

### User Story 3 - Operate after Azure deployment (Priority: P1)

As a product owner, I want the deployed application to use durable Azure-hosted storage so that accounts and user data survive application restarts and releases.

**Why this priority**: Deployment without a persistent production database would cause data loss or prevent authentication from working in Azure.

**Independent Test**: Deploy the application through the existing GitHub Actions workflow, complete registration and data creation, restart the App Service, and repeat the retrieval flow. Redeploy a no-op application change and repeat the retrieval flow again.

**Acceptance Scenarios**:

1. **Given** Azure contains a valid production database configuration and the released schema, **When** the application starts, **Then** it connects to the configured database and serves registration, login, and user-data operations without using local-development-only database settings.
2. **Given** a user has registered and saved data, **When** the App Service is restarted, **Then** the user can sign in and retrieve the same data.
3. **Given** a user has registered and saved data, **When** a later version is deployed successfully, **Then** the user can sign in and retrieve the same data unless an explicitly documented data migration changes that record.
4. **Given** a release build, test, configuration, or migration step fails, **When** the workflow completes, **Then** the deployment is reported as failed and the application is not represented as a successful release.

---

### User Story 4 - Validate production readiness through CI/CD (Priority: P2)

As a maintainer, I want the deployment workflow to build and test the application before deployment so that known regressions are blocked from Azure.

**Why this priority**: Automated verification reduces the risk of deploying an application that cannot start, authenticate, or use its database.

**Independent Test**: Run the workflow with a passing change and verify that checkout, SDK setup, restore, build, automated tests, local-mode validation, conditional external migration validation, packaging, deployment, Azure startup migration, readiness, and smoke testing complete in order. When a configured disposable MySQL-compatible validation target is present, external migration validation must run and failure must block publishing and deployment. When no configured disposable MySQL-compatible validation target is present, external migration validation must be recorded as skipped and must not block deployment. Run it with a deliberately failing test and verify that deployment does not run.

**Acceptance Scenarios**:

1. **Given** a change is merged into `master`, **When** the workflow runs, **Then** it checks out that commit, sets up the .NET SDK, restores dependencies, builds the application, runs automated tests and local-mode validation, validates the database migration strategy when a configured disposable MySQL-compatible validation target is present or records validation as skipped when no such target is present, and only then makes the deployment artifact available.
2. **Given** one or more automated tests fail, **When** the workflow runs, **Then** the workflow fails before the Azure deployment job starts.
3. **Given** all required verification steps pass and protected production configuration is available, **When** the deployment job runs, **Then** the artifact is deployed to the `PlantwiseWaterTracking` App Service.

---

### Edge Cases

- Registration uses an email address that differs only by letter casing or surrounding whitespace from an existing account.
- Registration submits a missing, malformed, or already-used email address.
- Registration submits a password that is too short, lacks required character categories, or contains unsupported input.
- Login submits an unknown email, incorrect password, empty fields, or excessive invalid attempts.
- A user submits an expired, missing, or invalid authentication/session token.
- A signed-in user attempts to access, update, or delete another user's record using a guessed identifier.
- A request is made after the user's account has been removed or disabled.
- The database is unavailable, times out, rejects credentials, reaches its connection limit, or returns a transient error.
- The database schema is behind the application version or a migration is partially applied.
- The production connection string is absent, malformed, points to the wrong database, or contains an invalid certificate setting.
- The App Service restarts during a write operation.
- A deployment is interrupted after the schema migration but before the application artifact is fully deployed.
- A redeployment contains no data migration but changes the application model.
- Concurrent requests from different users attempt to create or access records within the single active application instance; ownership and data isolation must remain intact.
- A future scale-out configuration would introduce multiple App Service instances or overlapping startup migrations; this is deferred and requires architectural reevaluation before the instance count increases above one.
- Local Development configuration is accidentally selected in Azure.
- Azure storage or the database reaches its configured cost, capacity, or retention limits.

## Requirements *(mandatory)*

### Authentication and Account Requirements

The authentication and user-data requirements in FR-001 through FR-015 are environment-scoped. In Development Mode, Visual Studio uses the existing JSON-backed and in-memory services; existing local behavior remains valid, persistent authentication storage is not required, and persistent user-data storage is not required. In Azure persistence mode, registration, authentication state, profile data, saved plants/favorites, and other user-owned data use persistent ASP.NET Core Identity and user-data storage in the Azure MySQL In-App database. Requirements involving authentication persistence, user-data persistence, relational schema, EF Core migrations, database availability, or relational durability apply to Azure persistence mode.

- **FR-001**: In Azure Mode, the system MUST allow a visitor to register with an email address and password. Development Mode may continue to use the existing non-persistent behavior and is not required to persist the account.
- **FR-002**: In Azure persistence mode, the system MUST validate email format, required fields, and password rules before creating an account. Development Mode retains existing validation behavior and does not require persistent account creation.
- **FR-003**: In Azure persistence mode, the system MUST enforce unique accounts by normalized email address.
- **FR-004**: In Azure persistence mode, the system MUST authenticate registered users with their valid credentials and establish an authenticated session.
- **FR-005**: In Azure persistence mode, the system MUST provide an explicit logout operation that invalidates the authenticated session.
- **FR-006**: The system MUST provide clear, non-sensitive feedback for successful and unsuccessful registration, login, and logout operations.
- **FR-007**: The system MUST NOT expose passwords, password hashes, security tokens, connection strings, or other credential material in pages, logs, validation responses, or exception messages.
- **FR-008**: Azure persistence mode MUST use the existing ASP.NET Core Identity architecture and must not require email verification, password reset, social login, multi-factor authentication, administrative dashboards, or role-based authorization in this feature. Development Mode does not require persistent Identity storage.

### User-Owned Data Requirements

- **FR-009**: In Azure persistence mode, the system MUST associate every persisted user-owned record with exactly one application user.
- **FR-010**: In Azure persistence mode, the system MUST require authentication before allowing a user to create, read, update, or delete user-owned data.
- **FR-011**: In Azure persistence mode, the system MUST scope every user-owned data query and mutation to the currently authenticated user's identity.
- **FR-012**: In Azure persistence mode, the system MUST deny unauthorized access to another user's records without revealing whether the target record exists.
- **FR-013**: In Azure persistence mode, the system MUST preserve ownership when a user updates an existing record and MUST prevent a client request from changing record ownership.
- **FR-014**: In Azure Mode, the system MUST persist valid user-owned data before reporting a successful save. Development Mode is not required to provide database-backed user-data persistence.
- **FR-015**: In Azure Mode, the system MUST return a safe, actionable error when a persistence operation fails and MUST not discard existing data silently. Development Mode must preserve the existing JSON/in-memory behavior and is not required to provide persistent-storage failure handling.

### Database and Persistence Requirements

- **FR-016**: Azure persistence mode MUST use the Azure-hosted production database rather than a local-development-only database configuration. Development Mode does not require a relational database.
- **FR-017**: Azure persistence mode's production database provider and schema MUST be compatible with the Azure-hosted database selected for the application; provider-specific schema artifacts MUST NOT be reused across incompatible database engines without validation.
- **FR-018**: The Azure persistence-mode database schema MUST include the structures required for Identity accounts, authentication state, account profile data including the default ZIP code, saved plants/favorites, timestamps, and their user ownership relationships. This relational schema requirement does not apply to Development Mode.
- **FR-019**: In Azure persistence mode, user-owned records MUST have a required ownership relationship to the user account, and the relationship MUST prevent orphaned records unless an explicit retention policy permits them.
- **FR-020**: Azure persistence mode MUST use a versioned database migration history to identify and apply schema changes. Development Mode is not expected to create or maintain a relational schema or migration history.
- **FR-021**: Azure App Service startup MUST validate `MYSQLCONNSTR_localdb` and apply only pending EF Core migrations through the configured Oracle MySql.EntityFrameworkCore provider before readiness or normal authenticated/data-dependent traffic is allowed; local startup MUST NOT execute database migrations.
- **FR-022**: Azure startup database initialization or migration MUST be safe to run more than once and MUST NOT recreate or erase existing user data. Development Mode does not create or migrate a relational schema.
- **FR-023**: In Azure persistence mode, the system MUST handle database unavailability without exposing provider details, SQL statements, credentials, or stack traces to users.
- **FR-024**: In Azure persistence mode, persistent user data MUST survive an application restart and a successful application redeployment.
- **FR-025**: Local development MUST remain usable without MySQL, SQLite, SQL Server, Docker, production credentials, or any relational database; it MUST use the existing JSON-backed and in-memory services, with no local authentication or user-data persistence requirement.

### Production Configuration Requirements

- **FR-026**: Azure production MUST receive the MySQL connection through the Azure App Service `MYSQLCONNSTR_localdb` connection-string setting or an equivalent protected Azure configuration that exposes that name.
- **FR-027**: For Azure persistence mode, no production connection string, password, access token, or database secret MAY be committed to the repository, workflow source, published artifacts, or application logs.
- **FR-028**: The application MUST fail clearly and safely when the required production database configuration is missing or invalid; it MUST NOT silently fall back to a local database in Azure.
- **FR-029**: Azure production configuration MUST identify the intended Azure MySQL In-App database through `MYSQLCONNSTR_localdb`; local developer-machine configuration MUST not be used by Azure.
- **FR-030**: Azure persistence mode configuration names and Azure resource names used by the application and workflow MUST consistently identify the `PlantwiseWaterTracking` App Service and its intended production database.

### CI/CD and Deployment Requirements

- **FR-031**: The existing GitHub Actions workflow MUST remain the deployment path for code merged into `master`.
- **FR-032**: The workflow MUST restore dependencies and build the application in a clean runner environment.
- **FR-033**: The workflow MUST run the complete unit-test suite before the deployment job can run.
- **FR-034**: GitHub Actions MUST stop before publishing or deployment when dependency restore, compilation, automated tests, workflow configuration checks that do not require production database credentials, or required deployment-target validation fail. When a configured disposable MySQL-compatible validation target is present, migration-artifact validation is required and a validation failure MUST block deployment; when no such target is present, validation MUST be recorded as skipped and deployment may continue.
- **FR-035**: The workflow MUST deploy only the intended published application artifact to the `PlantwiseWaterTracking` App Service and production slot.
- **FR-036**: The workflow MUST use least-privilege Azure authentication and MUST not print secret values.
- **FR-037**: When a configured disposable MySQL-compatible validation target is present, the workflow MUST validate MySQL migration artifacts against it; when no such target is present, the workflow MUST record the validation as skipped and continue without blocking deployment. The workflow MUST NOT apply production migrations; application startup in Azure MUST apply only pending migrations using protected configuration before readiness.
- **FR-038**: Deployment verification MUST confirm that the application starts, can reach the database, and can complete a representative authenticated persistence flow.
- **FR-039**: A failed deployment MUST be visible in GitHub Actions and MUST provide enough diagnostic information for maintainers to identify the failing stage without disclosing secrets or personal data.

### User Experience Requirements

- **FR-040**: Authentication pages MUST provide accessible labels, validation summaries, keyboard-operable controls, and clear success or failure feedback consistent with the existing application.
- **FR-041**: In Azure persistence mode, authenticated-only pages MUST direct unauthenticated visitors to the login flow or a safe access-denied response; Development Mode may retain its existing non-persistent access behavior.
- **FR-042**: Database and deployment failures MUST be presented as a generic service-availability or retry message rather than raw technical details.
- **FR-043**: The feature MUST not require a UI redesign unrelated to authentication or user data.

### Key Entities

- **Application User**: A registered person identified by a unique normalized email address and protected authentication credentials. The user owns their saved application records.
- **Authentication Session**: The server-recognized authenticated state established after successful login and ended by logout or expiration.
- **User-Owned Record**: A saved application record associated with exactly one application user and inaccessible to other users.
- **Database Schema Version**: The recorded version of the persistent schema required by the released application.
- **Production Configuration**: The environment-specific, protected settings that identify and authorize access to the Azure-hosted database and App Service.

## Explicit Acceptance Criteria

- **AC-001 Registration**: A valid unique registration creates exactly one account; an invalid or duplicate registration creates no additional account and displays field-level or summary validation.
- **AC-002 Login**: A valid account can log in within 5 seconds under normal test conditions; invalid credentials do not authenticate and do not disclose whether the email or password was incorrect.
- **AC-003 Logout**: After logout, an authenticated-only request is rejected or redirected to login within the next request.
- **AC-004 Password validation**: Password rules are enforced server-side for 100% of registration attempts, including requests that bypass client-side validation.
- **AC-005 User-specific access**: In a two-user test, each user can retrieve only their own records; attempts to access the other user's records are denied or return an indistinguishable unavailable response.
- **AC-006 Persistence after restart**: Records created before an App Service restart remain retrievable after restart with no manual reseeding or re-registration.
- **AC-007 Persistence after redeployment**: Records created before a successful redeployment remain retrievable after deployment of a later application artifact.
- **AC-008 Migration execution**: When a configured disposable MySQL-compatible validation target is present, the active MySQL migration set initializes a clean schema, applies supported upgrades, and safely reruns without duplicate or destructive changes; validation failure blocks publishing and deployment. When no configured disposable MySQL-compatible validation target is present, external migration validation is recorded as skipped, and Azure startup migration, readiness verification, and post-deployment smoke testing remain mandatory.
- **AC-009 Production deployment**: A successful workflow run has passing restore, build, unit-test, local-mode validation, packaging, deployment, Azure startup migration/readiness, and smoke-verification stages. When a configured disposable MySQL-compatible validation target is present, its validation must pass; when no configured disposable MySQL-compatible validation target is present, validation is recorded as skipped and deployment may continue. The application responds from the intended App Service.
- **AC-010 Connection string configuration**: The deployed application starts and connects using protected Azure configuration; no production credential exists in tracked source files or published application files.
- **AC-011 Unauthorized access protection**: Automated tests cover direct identifier manipulation, missing authentication, and cross-user requests, and all such attempts fail without exposing another user's data.
- **AC-012 Failure behavior**: Removing or invalidating `MYSQLCONNSTR_localdb` causes a clear failed health/deployment signal and does not cause the application to use local JSON/in-memory mode or silently report successful persistence.

## Given / When / Then Verification Scenarios

### Registration and Authentication

1. **Given** Azure persistence mode has completed mandatory startup migration and readiness, and an unregistered visitor, **When** the visitor submits a valid email and compliant password, **Then** one account is created and the visitor can log in. When a configured disposable MySQL-compatible validation target is present, migration validation must have passed before deployment; when no configured disposable MySQL-compatible validation target is present, external validation is recorded as skipped and deployment may continue.
2. **Given** Azure persistence mode has completed mandatory startup migration and readiness and an existing account, **When** a visitor submits the same email with a different password, **Then** no duplicate account is created and the response does not reveal account-enumeration details beyond the defined validation message. Azure startup migration, readiness verification, and post-deployment smoke testing remain mandatory.
3. **Given** a registered account, **When** the user submits an incorrect password, **Then** authentication fails, protected pages remain inaccessible, and no credential is logged.
4. **Given** an authenticated user, **When** the user logs out and requests a protected page, **Then** the request is redirected or denied and no protected data is returned.

### Ownership and Persistence

5. **Given** users A and B have separate accounts, **When** user A creates a record, **Then** user A can retrieve it and user B cannot retrieve it through normal navigation or a modified record identifier.
6. **Given** a saved record, **When** the application restarts, **Then** the record remains available after the owner signs in again.
7. **Given** a saved record, **When** a later version is deployed through the `master` workflow, **Then** the owner can still retrieve it after the deployment completes.

### Database and Release

8. **Given** a configured disposable MySQL-compatible validation target is present at the previous schema version, **When** the release migration is validated, **Then** the schema reaches the required version without deleting existing user accounts or records, and validation failure blocks publishing and deployment. **Given** no configured disposable MySQL-compatible validation target is present, **When** the release workflow runs, **Then** external migration validation is recorded as skipped and deployment may continue to mandatory Azure startup migration, readiness verification, and post-deployment smoke testing.
9. **Given** a configured disposable MySQL-compatible validation target is present where a migration has already been applied, **When** the same migration operation is validated again, **Then** it completes safely without creating duplicate schema objects or modifying user data, and validation failure blocks publishing and deployment. **Given** no configured disposable MySQL-compatible validation target is present, **When** the release workflow runs, **Then** external migration validation is recorded as skipped and deployment may continue. Azure startup migration, readiness verification, and post-deployment smoke testing remain mandatory in both cases.
10. **Given** a workflow change causes a unit test to fail, **When** the workflow executes, **Then** the deployment job is skipped or blocked and the workflow reports failure.
11. **Given** the Azure connection string is configured only in protected App Service settings, **When** the application starts in Azure, **Then** it uses that setting and no secret is present in repository contents, logs, or the published artifact.
12. **Given** the connection string is missing or the database is unreachable, **When** the application is deployed or checked, **Then** the release or health verification fails safely and users receive a generic unavailable message rather than provider details.

## Security Requirements

- Passwords MUST be handled only through the established Identity password-hashing and credential-validation mechanisms.
- Authentication cookies or equivalent sessions MUST be protected using the framework's secure defaults, transmitted only over HTTPS in production, and invalidated on logout.
- All authenticated user-data operations MUST enforce authorization on the server; hidden UI controls alone are insufficient.
- User identifiers and record identifiers supplied by clients MUST be treated as untrusted input.
- Anti-forgery protection MUST cover state-changing browser requests.
- Error responses and logs MUST omit passwords, password hashes, security stamps, authentication cookies, connection strings, database credentials, SQL statements, stack traces, and unrelated personal data.
- Database access MUST use encrypted transport where supported by the selected Azure database service and must validate the server certificate according to the supported production configuration.
- Production secrets MUST be stored in Azure App Service configuration, managed identity-compatible secret storage, or protected GitHub/Azure secret storage; they MUST not be placed in `appsettings.json` committed to GitHub.
- Azure deployment credentials MUST use least privilege and short-lived or federated authentication where supported by the existing workflow.
- The application MUST use generic responses for failed login and unauthorized record access to reduce account and record enumeration.

## Deployment Requirements

- The deployment target MUST be the existing Azure App Service named `PlantwiseWaterTracking` and its intended production slot.
- Deployment MUST occur through the existing GitHub Actions workflow after changes are merged into `master`.
- The workflow MUST run on a clean runner and must not depend on a developer's local relational database, local files outside the repository, or interactive credentials.
- The workflow MUST execute the exact sequence: checkout the merged `master` commit; set up the supported .NET 10 SDK; restore dependencies; build; run automated tests; run local-mode validation tests; validate the MySQL migration model when a configured disposable MySQL-compatible validation target exists, or record validation as skipped when no target is configured; publish the application; deploy to `PlantwiseWaterTracking`; let Azure startup validate configuration; apply pending migrations; wait for Azure readiness; and run a required post-deployment smoke test. The workflow succeeds or fails based on the smoke-test result and earlier required gates.
- Startup migration MUST complete before readiness or normal authenticated/data-dependent traffic. Startup or migration failure MUST make readiness and the smoke test fail, and the workflow MUST be unsuccessful.
- Azure App Service application settings or connection-string settings MUST provide the production database configuration at runtime.
- The deployed application MUST use `MYSQLCONNSTR_localdb` in the Azure environment and MUST fail readiness when it is missing; it MUST not fall back to local JSON/in-memory mode or a developer database in Azure.
- The workflow MUST verify that its configured App Service name matches the actual deployment target and must not silently deploy to a different resource or slot.
- Deployment logs MUST identify the stage and outcome while redacting secrets and personal data.
- The release process MUST document rollback or recovery handling for an application artifact that cannot start after a schema-compatible deployment.

## Database Requirements

- The selected Azure-hosted database MUST be Azure MySQL In-App for the initial free minimum viable product and must be supported for the chosen Entity Framework provider.
- The MVP MUST document and accept the durability, backup, scaling, availability, and operational limitations of MySQL In-App; a later move to a separately managed Azure database remains a possible follow-up.
- The production provider, connection-string format, migration artifacts, and deployment checks MUST all target the same database engine. A SQL Server migration MUST NOT be assumed to run on MySQL.
- The database MUST store Identity account data and all in-scope user-owned records required for post-deployment operation.
- User-owned data MUST have a required foreign-key-like ownership relationship and an index or equivalent lookup strategy that supports filtering by authenticated user.
- The database MUST enforce uniqueness for normalized email addresses and any other business keys defined for user-owned records.
- Schema changes MUST be represented by reviewed, versioned migrations. GitHub Actions validates them only; application startup applies pending production migrations through the configured MySQL provider.
- Migration application MUST be observable, repeatable, and non-destructive to existing user data.
- The application MUST NOT use `EnsureCreated` or another shortcut that bypasses EF Core migration history, and production migrations MUST NOT be automatically rolled back. Application rollback is permitted only when the earlier artifact is compatible with the already-applied schema.
- The MVP MUST use one active Azure App Service instance; scale-out is disabled or remains at one instance, overlapping startup migration is not approved, provider-supported migration locking is used when available, and startup migration must be reconsidered before horizontal scaling.
- Azure production persistence MUST use Azure MySQL In-App through the protected `MYSQLCONNSTR_localdb` setting. Local development MUST not require or create a relational database. External initialization, upgrade, and rerun checks execute only when a configured disposable MySQL-compatible validation target is present; without that target, the checks are recorded as skipped. Azure startup migration, readiness verification, and smoke testing are always required.
- The database configuration MUST define supported connection timeout, retry, encryption, and pooling behavior appropriate for the selected Azure service without logging credential-bearing values.
- The application MUST remain functional for the in-scope data when the application process is restarted or a new application artifact is deployed.

## Verification Criteria

The feature is ready for implementation review only when all of the following evidence is available:

1. A local-mode validation proves the application starts and supports its JSON-backed/in-memory flows without any relational database or startup migration.
2. An Azure-like registration test creates a valid account and rejects invalid, duplicate, and non-compliant registrations.
3. Login and logout tests prove both successful authentication and loss of access after logout in the Azure persistence mode.
4. Server-side password validation tests cover every required password rule and bypass attempts that do not use browser validation.
5. User-ownership tests prove that two Azure-persisted accounts cannot read, update, or delete each other's records, including direct identifier manipulation.
6. An Azure persistence test creates data, restarts the application, and retrieves the same data.
7. An Azure redeployment test creates data, deploys a later artifact through GitHub Actions, and retrieves the same data afterward.
8. Migration evidence MUST show one of the following:
	  a. When a configured disposable MySQL-compatible validation target is present, evidence must show successful clean initialization, supported upgrade, and safe rerun validation against that target.
   b. When no configured disposable MySQL-compatible validation target is present, evidence must show a recorded external-validation skip together with successful Azure startup migration, readiness verification, and post-deployment smoke testing.
   Azure startup migration, readiness verification, and post-deployment smoke testing are mandatory in both cases.
9. CI/CD evidence shows restore, compilation, all unit tests, local-mode validation, packaging, deployment, Azure startup migration, readiness, and post-deployment smoke verification in the required order. When a disposable MySQL-compatible validation target is configured, migration validation is also shown as passed; when no target is configured, the evidence shows validation recorded as skipped and deployment continuing. Azure startup migration, readiness, and smoke testing are always required.
10. Production configuration evidence shows the database connection is injected through Azure-protected configuration and is absent from tracked files and published artifacts.
11. A negative Azure configuration test proves that a missing or invalid `MYSQLCONNSTR_localdb` does not report readiness or select local JSON/in-memory mode.
12. A production smoke test confirms the application starts at the intended `PlantwiseWaterTracking` App Service and completes registration, login, save, logout, login again, and retrieval.
13. Security review evidence confirms no sensitive credential, connection string, password hash, token, SQL statement, or stack trace is exposed in user-facing responses or deployment logs.

## Non-Functional Requirements

- **NFR-001 Availability**: During normal Azure service operation, authenticated users MUST be able to retrieve previously saved data after application restart or redeployment.
- **NFR-002 Performance**: In the defined production-like measurement environment, the home, registration, login, authenticated profile/dashboard, and representative save-and-retrieve operation MUST complete within 3 seconds for at least 95% of 10 post-warm-up runs per operation; this is an end-to-end page/request measurement, not an enterprise load test.
- **NFR-003 Reliability**: A transient database failure MUST produce a safe failure response and MUST NOT report a write as successful unless persistence has completed.
- **NFR-004 Data durability**: A successful save MUST remain available after at least one application restart and one subsequent successful deployment verification.
- **NFR-005 Security**: No production secret or credential-bearing connection string MAY appear in version control, build artifacts, user-facing responses, or ordinary workflow logs.
- **NFR-006 Maintainability**: Database schema changes MUST be traceable to a versioned migration and a release that depends on that migration.
- **NFR-007 Compatibility**: The solution MUST remain compatible with the project's .NET 10 Razor Pages architecture, existing application conventions, GitHub source control, and Azure App Service deployment model.
- **NFR-008 Cost**: The selected Azure database and App Service configuration SHOULD use the lowest practical tier that meets persistence, availability, security, and expected usage requirements; cost-impacting choices MUST be documented.
- **NFR-009 Observability**: Build, test, migration, deployment, startup, and database-availability failures MUST be distinguishable in authorized diagnostic output without exposing secrets or personal data.
- **NFR-010 Recovery**: The release process MUST document how to identify a failed migration or incompatible artifact and how to restore service without deleting user data.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 100% of valid registration, login, logout, and password-validation acceptance tests pass in the automated test suite.
- **SC-002**: In a two-user authorization test suite, 100% of cross-user read, update, and delete attempts are denied without exposing the target record.
- **SC-003**: 100% of records successfully saved before an application restart remain retrievable after restart in the production-like verification environment.
- **SC-004**: 100% of records successfully saved before a verified redeployment remain retrievable after redeployment.
- **SC-005**: In Azure persistence mode, when a configured disposable MySQL-compatible validation target is present, it can be initialized and upgraded by the documented migration process with zero intentional data loss in validation tests; when no configured disposable MySQL-compatible validation target is present, validation is recorded as skipped and Azure startup migration, readiness verification, and post-deployment smoke testing remain mandatory.
- **SC-006**: A workflow with a failing unit test produces zero Azure deployment attempts for that run.
- **SC-007**: A successful production workflow contains passing restore, build, unit/integration-test, local-mode validation, publish, deployment, Azure startup/readiness, and required smoke-verification results for the intended App Service. When a configured disposable MySQL-compatible validation target is present, its validation also passes; when no such target is present, validation is recorded as skipped and deployment may continue. CI has not applied production migrations.
- **SC-008**: 100% of inspected repository files, published artifacts, and ordinary CI/CD logs contain no production database secret or password.
- **SC-009**: At least 95% of representative authentication and saved-data retrieval operations complete within 3 seconds under expected project usage in a production-like environment.
- **SC-010**: A missing or invalid production database configuration is detected before a false successful persistence flow can be reported.
- **SC-011**: 100% of Azure database-negative checks for missing `MYSQLCONNSTR_localdb`, invalid credentials, unreachable database, and migration failure produce failed startup/readiness and failed smoke-test workflow status without local-mode fallback or sensitive output.
- **SC-012**: The defined WCAG 2.1 AA MVP checks pass for all six authentication and user-data experiences.
- **SC-013**: Local-mode validation completes without a relational database, database connection string, or startup migration.

## Risks and Mitigations

- **Risk**: The selected Azure-hosted database engine differs from the provider and migrations currently used by the application. **Mitigation**: Confirm the production engine before implementation; generate and validate migrations for that engine rather than assuming cross-provider compatibility.
- **Risk**: MySQL In-App may not provide the durability, scaling, backup, or SLA expected beyond a free MVP. **Mitigation**: Document the service limitations, keep the MVP data scope appropriate, and identify migration to a managed Azure database as a follow-up if usage or availability needs increase.
- **Risk**: Database migrations and application deployment can become incompatible during a release. **Mitigation**: Use a reviewed migration sequence, compatibility checks, ordered release stages, and documented recovery handling.
- **Risk**: A connection string can be accidentally committed or emitted in logs. **Mitigation**: Use protected Azure settings or secret storage, repository scanning, redaction, and negative verification tests.
- **Risk**: Missing ownership filters could expose another user's data. **Mitigation**: Enforce ownership at every data access boundary and include direct identifier-manipulation tests.
- **Risk**: The workflow may deploy to a different App Service or slot than intended. **Mitigation**: Verify the configured resource name, slot, subscription, and post-deployment endpoint before enabling production release.
- **Risk**: Azure may omit or misname `MYSQLCONNSTR_localdb`. **Mitigation**: Make the required production setting explicit and fail safely rather than silently falling back to local settings.
- **Risk**: CI/CD verification may pass while the production database is unavailable. **Mitigation**: Add a protected, non-destructive migration/database connectivity check and a post-deployment authenticated smoke test.
- **Risk**: Package vulnerabilities or unsupported dependency versions may block or weaken production readiness. **Mitigation**: Review restore warnings and resolve or explicitly accept them before release.

## Assumptions

- The existing Azure App Service named `PlantwiseWaterTracking` is available and the team has permission to configure it.
- The existing GitHub Actions workflow is the authoritative deployment mechanism and changes will be merged from the feature branch into `master`.
- The application will continue to use email/password authentication and ASP.NET Core Identity; social login and multi-factor authentication remain out of scope.
- New users will be allowed to log in immediately after successful registration; email delivery and email verification are not required for the free MVP.
- Password reset is excluded from the free MVP; users who lose access to their credentials will require a separately approved recovery feature.
- Azure MySQL In-App is the selected database for the free MVP. This specification does not assume that the existing SQL Server migration can run on MySQL; MySQL-compatible provider configuration and schema validation are required.
- Production credentials and connection strings can be supplied through protected Azure or CI/CD configuration without committing them to GitHub.
- Local development uses only the existing JSON-backed and in-memory services and does not require authentication or user-data persistence. A configured disposable MySQL-compatible validation target may be supplied for external migration validation but is not required for the student-project deployment. Its absence produces a recorded validation skip rather than a release failure. Azure MySQL In-App remains the production persistence environment.
- Existing plant and location source data may remain outside the relational user-data store unless a separate approved feature expands that scope. The MVP persists account profile data including the default ZIP code, saved plants/favorites, and associated timestamps; watering calculation history and general preferences are not required.
- Users have network access to the deployed application and use supported browsers with cookies and HTTPS enabled.
- Account and user-owned data retention is indefinite for the life of the application unless a later approved retention policy is introduced.
- No administrative or role-based management workflow is required for the initial release.

