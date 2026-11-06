# Feature Specification: Authenticated User Accounts and Personal Plants

**Feature Branch**: `002-user-accounts-plants`
**Created**: 2026-11-03
**Status**: Draft
**Input**: Add authenticated accounts, a persisted default USA ZIP code, personal plant history, favorites, and saved-plant watering recommendations to PlantWise.

## Feature Purpose

PlantWise currently lets a visitor search for a plant and ZIP code and receive a watering recommendation. This feature adds private user accounts and persistent personalization so a registered user can save one default location, revisit successfully searched plants, favorite frequently used plants, and request a current recommendation without re-entering the ZIP code.

The existing plant-search and watering-calculation behavior remains available according to its current access rules. This feature reuses the existing deterministic mock-weather inputs and `WateringCalculatorService`; external weather API integration is a separate future feature. This specification defines user-visible behavior and approved feature constraints.

## User Scenarios & Testing

### User Story 1 - Register an account (Priority: P1)

As a visitor, I want to create an account with a valid default ZIP code so PlantWise can personalize future watering recommendations.

**Why this priority**: Account creation is the prerequisite for private persistence and personalization.

**Independent Test**: Submit valid registration information and a ZIP present in the application's ZIP-location dataset, then sign in and verify the account's default ZIP is available.

**Acceptance Scenarios**:

1. **Given** a visitor provides valid required registration information and a five-digit ZIP present in the ZIP-location dataset, **When** the visitor submits registration, **Then** an account is created, the ZIP is saved as the default location, and the visitor receives a clear success path to sign in.
2. **Given** the supplied ZIP is not found in the ZIP-location dataset, **When** the visitor submits registration, **Then** registration is rejected with clear ZIP validation feedback and no account is created or partially saved.
3. **Given** the ZIP is missing, malformed, or contains a non-five-digit value, **When** the visitor submits registration, **Then** registration is rejected with field-level feedback and the ZIP is not saved.

### User Story 2 - Sign in and sign out privately (Priority: P1)

As a registered user, I want to sign in and sign out securely so my plant information remains private.

**Why this priority**: Authentication protects all personal settings, saved plants, and favorites.

**Independent Test**: Register a user, sign in with valid credentials, access a protected personal page, sign out, and verify that the protected page is no longer accessible.

**Acceptance Scenarios**:

1. **Given** a registered account and correct credentials, **When** the user signs in, **Then** the user is authenticated and can access personal account and plant pages.
2. **Given** incorrect or incomplete credentials, **When** a visitor attempts to sign in, **Then** sign-in fails with a generic message that does not reveal whether the account or identifier exists.
3. **Given** a signed-in user, **When** the user signs out, **Then** the session ends and subsequent access to protected personal pages requires authentication.
4. **Given** a signed-out visitor, **When** the visitor requests a protected personal page or action, **Then** access is denied or redirected to sign-in without revealing personal data.

### User Story 3 - Manage the default ZIP code (Priority: P1)

As a signed-in user, I want my default ZIP code supplied automatically and editable so recommendations reflect my current location.

**Why this priority**: The default location removes repeated input while keeping recommendations relevant.

**Independent Test**: Sign in with a saved ZIP, request a recommendation without entering a ZIP, update the ZIP to another supported value, and verify later recommendations use the new ZIP.

**Acceptance Scenarios**:

1. **Given** a signed-in user with a saved default ZIP, **When** the user requests a recommendation for a saved plant, **Then** PlantWise automatically uses the saved ZIP without requiring another ZIP entry.
2. **Given** a signed-in user submits a different five-digit ZIP present in the ZIP-location dataset, **When** the user saves the profile update, **Then** the new ZIP replaces the prior default and is used for future recommendations.
3. **Given** a ZIP update is missing, malformed, or absent from the ZIP-location dataset, **When** the user submits the update, **Then** the update is rejected with clear feedback and the previous valid ZIP remains unchanged.

### User Story 4 - Build a personal plant list (Priority: P1)

As a signed-in user, I want plants I successfully search for added to my personal plant list so I can revisit them.

**Why this priority**: Persistent plant history is the core personalization value of the feature.

**Independent Test**: Sign in, complete a successful plant search, open the personal plant page, sign out and back in, and verify the plant remains listed once.

**Acceptance Scenarios**:

1. **Given** a signed-in user successfully searches for a plant in the existing PlantWise dataset, **When** the search and recommendation succeed, **Then** that plant is added to the signed-in user's personal list.
2. **Given** a signed-in user searches successfully for the same plant again, **When** the search completes, **Then** the personal list contains one entry for that plant rather than a duplicate.
3. **Given** a signed-in user has no saved plants, **When** the user opens the personal plant page, **Then** an empty state explains that no plants have been searched for or saved yet.
4. **Given** a saved plant no longer exists in the source plant dataset, **When** the user opens the personal plant page, **Then** the page remains usable and clearly identifies the unavailable plant without failing the entire list.
5. **Given** a signed-in user signs out and later signs back in, **When** the user opens the personal plant page, **Then** the previously saved plants remain available.

### User Story 5 - Favorite personal plants (Priority: P2)

As a signed-in user, I want to favorite and unfavorite plants so frequently used plants appear at the top of my list.

**Why this priority**: Favorites improve access to frequently used saved plants after the core list is available.

**Independent Test**: Save at least two plants, favorite one, verify ordering and visible status, unfavorite it, and verify the status and ordering change.

**Acceptance Scenarios**:

1. **Given** a saved plant belongs to the signed-in user, **When** the user marks it as favorite, **Then** the plant shows an accessible visible favorite indication and appears before non-favorite plants.
2. **Given** a saved plant is favorited by the signed-in user, **When** the user removes favorite status, **Then** the visible indication is removed and the plant is treated as non-favorite.
3. **Given** a user has favorited a plant, **When** the user signs out and later signs back in, **Then** the favorite status remains.
4. **Given** two users have saved the same source plant, **When** one user favorites it, **Then** only that user's personal entry is changed.

### User Story 6 - Open a saved plant recommendation (Priority: P1)

As a signed-in user, I want to select a saved plant and see its current watering needs using my default ZIP code.

**Why this priority**: This completes the personalized return journey and reuses the established recommendation behavior.

**Independent Test**: Select a saved plant with a valid default ZIP and verify the result identifies the plant, ZIP, calculated need, and existing explanation.

**Acceptance Scenarios**:

1. **Given** a signed-in user selects one of the user's saved plants and has a valid default ZIP, **When** the user opens the plant, **Then** PlantWise uses the selected plant, the authenticated user's current default ZIP code, the existing ZIP-location reference dataset, the existing deterministic mock-weather inputs, the existing `WateringCalculatorService`, and the existing PlantWise watering calculation and recommendation explanation to display a recommendation.
2. **Given** the user changes the default ZIP, **When** the user later opens the same saved plant, **Then** PlantWise recalculates the recommendation using the new ZIP rather than a stale location.
3. **Given** the required plant or ZIP-location data is unavailable, or the existing deterministic mock-weather inputs are unavailable or invalid, **When** the user opens the saved plant, **Then** a safe understandable error is shown, no recommendation is displayed, no personal plant record is created or changed, existing favorite status and account/default-ZIP data remain unchanged, no external weather provider is contacted, and the user can continue using the application.

### User Story 7 - Preserve the existing visitor journey (Priority: P1)

As a user, I want existing plant search and watering behavior to remain functional while accounts are added.

**Why this priority**: Personalization must not regress the application's existing value for visitors or existing users.

**Independent Test**: Exercise the existing plant-and-ZIP search flow under its current access rules before and after authentication feature behavior is enabled.

**Acceptance Scenarios**:

1. **Given** a visitor uses the existing plant-search flow with valid input, **When** the visitor submits the search, **Then** the existing watering recommendation behavior remains available according to current access rules.
2. **Given** an existing search has invalid input or unavailable data, **When** the user submits it, **Then** the existing validation and safe failure behavior remains clear and usable.

## Functional Requirements

- **FR-001**: The system MUST allow a visitor to create an account using the required registration information and a valid default five-digit ZIP code.
- **FR-001a**: Registration and sign-in MUST use an email address and password.
- **FR-001b**: Registration MUST reject an email address already associated with an account using a generic already-registered message that does not expose account details.
- **FR-002**: The system MUST allow a registered user to sign in and sign out.
- **FR-003**: The system MUST delegate password hashing, verification, password-policy enforcement, account lockout behavior when enabled, and safe authentication failures to ASP.NET Core Identity. The configured password policy MUST explicitly require `RequiredLength = 8`, `RequiredUniqueChars = 1`, `RequireDigit = true`, `RequireLowercase = true`, `RequireUppercase = true`, and `RequireNonAlphanumeric = false`. The application MUST protect passwords from display, readable storage, and logging and MUST NOT implement custom password validation, readable-password storage, or custom password hashing.
- **FR-004**: The system MUST use a generic authentication failure message that does not reveal whether a particular account exists.
- **FR-005**: The system MUST persist one default ZIP code for each user.
- **FR-006**: The system MUST verify that a default ZIP code is exactly five digits and exists in the application's ZIP-location dataset before saving it.
- **FR-007**: The system MUST allow a signed-in user to view and update that user's default ZIP code.
- **FR-008**: The system MUST automatically use the signed-in user's current default ZIP code when the user requests a recommendation for a saved plant.
- **FR-009**: The system MUST preserve the prior valid default ZIP when an attempted update is invalid or unavailable.
- **FR-010**: The system MUST add a successfully searched existing plant to the signed-in user's personal plant list.
- **FR-011**: The system MUST persist each personal plant list across sign-out and later sign-in.
- **FR-012**: The system MUST associate each personal list entry with an existing PlantWise dataset plant and MUST prevent duplicate entries for the same user and plant.
- **FR-013**: The system MUST display an understandable empty state when a user has no personal plants.
- **FR-014**: The system MUST keep the personal plant page usable when a previously saved plant is unavailable in the current source dataset.
- **FR-015**: The system MUST allow a signed-in user to mark and unmark that user's saved plants as favorites.
- **FR-016**: The system MUST persist favorite status and display favorite plants before non-favorite plants.
- **FR-017**: The system MUST provide an accessible visible indication of favorite status that is not conveyed only through color or icon shape; each server-rendered control MUST have visible text or an accessible label stating Favorite or Not favorite, an action name such as Add [plant name] to favorites or Remove [plant name] from favorites, correct button semantics, and keyboard operability without JavaScript.
- **FR-018**: The system MUST allow a signed-in user to open a saved plant and view a current recommendation using the user's current default ZIP and the established PlantWise calculation.
- **FR-019**: Recommendation results MUST identify the current plant common name, current scientific name when available, ZIP code used, calculated water need and units, and the existing PlantWise explanation; the result MUST not expose internal database identifiers and MUST identify the existing deterministic mock-weather inputs when the current UI exposes data-source context.
- **FR-020**: The system MUST show a clear safe error when required plant or ZIP-location data is unavailable, or the existing deterministic mock-weather inputs are unavailable or invalid, and MUST keep the application usable without creating or changing a personal plant record.
- **FR-021**: The system MUST preserve the existing visitor plant-search and watering behavior according to current access rules.
- **FR-022**: The system MUST require authentication for personal account, personal plant, favorite, and default-ZIP actions.
- **FR-023**: The system MUST authorize every personal read and write against the signed-in user's identity; a user MUST NOT view or modify another user's account, saved plant, or favorite data.
- **FR-024**: The system MUST validate user-provided account, ZIP, plant-selection, and action values at the point of use.
- **FR-025**: Passwords, password hashes, security stamps, authentication cookies, connection strings, secrets, antiforgery tokens, full account records, and unrelated personal data MUST NOT be written to logs or error responses. Expected authentication failures MUST NOT log submitted credentials. User IDs MAY be used internally for authorization and correlation, but logs MUST omit unnecessary profile information. User-facing exceptions MUST be safe, with detailed exception data limited to appropriate development diagnostics; production-style behavior MUST not expose stack traces or database details. Registration validation MUST use the configured ASP.NET Core Identity password policy and safe messages without echoing passwords.
- **FR-026**: The system MUST use the existing deterministic mock-weather inputs and `WateringCalculatorService` for saved-plant recommendations; this feature MUST NOT add or call Open-Meteo, Visual Crossing, or another external weather provider.
- **FR-027**: A signed-in user's personal plant MUST be upserted only after ZIP resolution, plant resolution, and a valid recommendation from `WateringCalculatorService` succeed; failed validation, lookup, or calculation MUST not create or alter the record.
- **FR-028**: A personal plant MUST retain SourcePlantId, CommonNameSnapshot, ScientificNameSnapshot, authenticated user ownership identifier, IsFavorite, CreatedAt, and LastSearchedAt as a UTC timestamp, without copying the plant water factor as authoritative data. CreatedAt is set only on first successful addition and never changes during repeat searches or favorite toggles. LastSearchedAt is set after the first successful recommendation and updated after each later successful recommendation for the same user and SourcePlantId; it is not updated by failed operations, list viewing, favorite toggles, or unavailable-source rendering.

## Validation Rules

- Registration and default-ZIP updates require a nonempty value of exactly five digits.
- The ZIP must resolve to a record in the current ZIP-location dataset; format validity alone is insufficient.
- Plant searches must continue to use the existing PlantWise matching behavior and only existing dataset plants may be saved.
- Repeated searches are idempotent for a given user and source plant.
- Favorite and saved-plant actions must identify a valid personal entry belonging to the signed-in user.
- Failed validation MUST not create an account, change the default ZIP, create a duplicate plant, or change another user's favorite status.

## Authorization and Privacy

- Personal account settings, default ZIP data, saved plants, and favorites are private to the owning authenticated user.
- A signed-out visitor cannot access protected personal pages or actions.
- Attempted access to another user's saved plant record MUST be denied without exposing whether the record exists.
- Authentication and authorization failures MUST use safe, non-sensitive messages.
- Passwords, credentials, and personal data MUST not be logged or displayed as readable text.

## Persistence Expectations

- Account identity, credential verification data, default ZIP, personal plant associations, and favorite status persist beyond the current request and survive sign-out and later sign-in.
- Updating a default ZIP changes future recommendations but does not rewrite the identity of previously saved plants.
- Personal plant entries refer to the corresponding existing source plant conceptually; unavailable source records remain represented safely rather than causing the personal list to fail.

## Conceptual Data Entities

- **User Account**: A registered identity with credential-verification information and one current default ZIP code.
- **ZIP Location**: An existing PlantWise location record that validates a five-digit ZIP and supplies location data for recommendations.
- **Plant**: An existing PlantWise source-dataset record used by searches and calculations.
- **Personal Plant**: A user's saved association to one source plant, including favorite status and enough identity to handle source-record unavailability safely.
- **Watering Recommendation**: The existing PlantWise result containing the selected plant, ZIP used, calculated water need, and explanation; recommendations are generated when requested rather than treated as permanent saved values.

## Dependencies on Existing PlantWise Behavior

- Existing plant search and matching rules remain authoritative.
- Existing nationwide ZIP-location data remains authoritative for default-ZIP validation and location resolution.
- Existing deterministic mock-weather inputs remain authoritative for current inputs in this feature.
- Existing `WateringCalculatorService`, units, rounding/display behavior, and explanation remain unchanged.
- Existing safe unavailable-data and validation behavior remains the baseline for new account and personal-plant flows.

## Edge Cases

- Registration uses a validly formatted but unknown ZIP.
- Registration or ZIP update contains whitespace, letters, punctuation, fewer than five digits, or more than five digits.
- The user's current default ZIP later disappears from the ZIP-location dataset.
- A saved plant is removed, renamed, or otherwise unavailable in the source plant dataset; the personal record remains visible with its retained name snapshots, is marked unavailable, preserves favorite state, and cannot be opened for a current recommendation.
- Existing deterministic mock-weather inputs are unavailable or invalid while opening a saved plant; no external weather provider is contacted.
- A user submits the same successful search repeatedly or submits favorite/unfavorite actions repeatedly.
- Two users save the same plant and one changes favorite status.
- A signed-out visitor attempts direct access to a personal URL or action.
- A user changes an identifier in a personal-plant request to target another user's record.
- Registration uses an email address already associated with an account; the system rejects it with a generic already-registered message without exposing account details.
- Registration and sign-in use an email address and password. ASP.NET Core Identity enforces the approved password policy: `RequiredLength = 8`, `RequiredUniqueChars = 1`, `RequireDigit = true`, `RequireLowercase = true`, `RequireUppercase = true`, and `RequireNonAlphanumeric = false`. The application does not implement custom password hashing or verification; passwords are never stored as readable text, written to logs, or returned in validation responses. Registration displays safe policy-failure messages, and authentication failures use a generic message that does not reveal whether an account exists.

## Scope Boundaries

### In Scope

- Account registration, sign-in, and sign-out.
- One persisted default USA ZIP per user.
- Private personal plant history from successful searches.
- Duplicate prevention for a user's saved plant entries.
- Favorite and unfavorite actions with persistent ordering.
- Opening saved plants for current recommendations using the existing behavior.
- Clear validation, authorization, privacy, and unavailable-data feedback.

### Out of Scope

- Google, Microsoft, Facebook, or other social sign-in providers.
- Administrator account management.
- Sharing plant lists or favorites between users.
- Public profiles or social features.
- Email marketing.
- Watering reminders, scheduled notifications, or push notifications.
- Multiple saved ZIP codes per user.
- Manually adding plants absent from the PlantWise dataset.
- Changing the established watering formula.
- Replacing or changing the existing deterministic mock-weather inputs, or introducing external weather providers or live weather API integrations, is outside the scope of feature 002. Feature 002 does not add live evapotranspiration or rainfall retrieval, does not create a second watering calculation, and continues to reuse the existing PlantWise watering recommendation behavior.
- Redesigning unrelated pages.

## Assumptions

- The existing application continues to support its current visitor search access rules.
- A user has exactly one current default ZIP; changing it affects future recommendation requests.
- A plant is added to the personal list only after a successful existing plant search and recommendation flow.
- Personal list order is favorites first; within each favorite group, a stable deterministic order is sufficient unless clarified later.
- Current recommendations are recalculated when opened and are not assumed to be permanently accurate after weather changes.
- Standard ASP.NET Core Identity authentication is used with the approved password policy: `RequiredLength = 8`, `RequiredUniqueChars = 1`, `RequireDigit = true`, `RequireLowercase = true`, `RequireUppercase = true`, and `RequireNonAlphanumeric = false`. This policy is approved for feature 002 and is not deferred to implementation planning. Password hashing, verification, policy enforcement, and safe authentication failures remain delegated to Identity; passwords are never stored as readable text, logged, or returned in validation responses.
- The existing source datasets and deterministic mock-weather inputs remain available to the feature.

## Success Criteria

- **SC-001**: At least 95% of valid registration attempts with supported ZIP codes complete successfully within 10 seconds under normal application conditions.
- **SC-002**: At least 95% of valid sign-in attempts complete within 5 seconds under normal application conditions, and 100% of invalid attempts return a generic failure message without account-existence disclosure.
- **SC-003**: In acceptance testing, 100% of protected personal-page and personal-action cases prevent signed-out access and cross-user access.
- **SC-004**: In acceptance testing, 100% of successful plant searches by signed-in users produce exactly one personal-list entry per user and source plant, including repeated searches.
- **SC-005**: In acceptance testing, favorite status remains correct after sign-out and later sign-in, and all favorite plants appear before non-favorites.
- **SC-006**: In acceptance testing, 100% of saved-plant recommendation results identify the selected plant, ZIP used, calculated need, and existing explanation when required data is available.
- **SC-007**: In acceptance testing, invalid or unavailable account, ZIP, plant, or existing deterministic mock-weather inputs produce an actionable safe message without corrupting existing valid personal data.
- **SC-008**: Existing plant-search and watering regression tests continue to pass with no change to the established calculation formula or displayed units.

### Performance Measurement Conditions

For SC-001 and SC-002, normal application conditions mean local development or automated integration-test execution with SQL Server LocalDB on the same development machine, Release configuration, no external weather calls, deterministic mock-weather inputs, one active test user, and only feature-test data seeded. Use one warm-up request followed by at least 10 measured executions per operation. Measure elapsed time at the application request boundary, report median and slowest execution, and exclude startup, first migration, and debugger overhead.
