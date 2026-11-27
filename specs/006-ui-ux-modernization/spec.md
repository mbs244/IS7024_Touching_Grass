# Feature Specification: UI Consolidation and User Experience Modernization

**Feature Branch**: `[006-ui-ux-modernization]`

**Created**: 2026-11-27

**Status**: Draft

**Input**: User description: "UI Consolidation and User Experience Modernization"

## Clarifications

### Session 2026-11-27

- Q: For plant autocomplete matching, which rule should be canonical? → A: Case-insensitive contains
- Q: How many autocomplete suggestions should be shown at once? → A: Top 10
- Q: What browser title should the unified landing page use? → A: Plantwise - Watering Guide

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Unified Landing Watering Guide (Priority: P1)

As a visitor or signed-in user, I can enter or confirm ZIP code and plant selection directly on the landing page, calculate watering recommendations there, and see results without being sent to a separate workflow page.

**Why this priority**: This is the core user outcome and removes the largest current usability friction.

**Independent Test**: Open the landing page, enter a valid ZIP code and plant, select Calculate, and confirm recommendations display on the same page with no redirect.

**Acceptance Scenarios**:

1. **Given** I am on the landing page, **When** the page loads, **Then** I can enter a ZIP code, search for a plant, and calculate watering recommendations from the same page.
2. **Given** I provide valid ZIP code and plant selection, **When** I select Calculate, **Then** watering recommendations display in place on the landing page without a separate workflow redirect.
3. **Given** legacy duplicate data-entry flow exists, **When** this feature is delivered, **Then** duplicate watering-guide input entry points are removed from the user experience.

---

### User Story 2 - Faster Plant Selection with Suggestions (Priority: P2)

As a user searching for a plant, I receive matching plant suggestions while typing so I can select the correct plant without relying on exact-name matches.

**Why this priority**: Plant lookup accuracy directly affects successful recommendation completion.

**Independent Test**: Type at least three characters into plant search, verify matching options show common and scientific names, select one result, then calculate recommendations successfully.

**Acceptance Scenarios**:

1. **Given** plant data is available, **When** I type three or more characters, **Then** case-insensitive contains matches appear in a suggestion dropdown.
2. **Given** multiple plants match entered text, **When** suggestions are shown, **Then** each item includes common and scientific names.
3. **Given** I type "Maple", **When** suggestions load, **Then** all matching maple species are available.
4. **Given** I select a suggested plant, **When** selection is completed, **Then** the existing watering-guide plant identifier is populated exactly.
5. **Given** no plant matches entered text, **When** search completes, **Then** a user-friendly no-results message appears.

---

### User Story 3 - Consistent Plantwise Experience Across Pages (Priority: P3)

As a user navigating the application, I see consistent branding, titles, navigation labels, and concise messaging so the product feels clear and cohesive.

**Why this priority**: Consistency improves trust and discoverability but depends on core interaction flow already being available.

**Independent Test**: Navigate all pages and verify shared visual style, user-friendly navigation labels, Plantwise-prefixed browser titles, and concise landing-page messaging.

**Acceptance Scenarios**:

1. **Given** I navigate between pages, **When** each page renders, **Then** layout, spacing, typography, and interaction styling are visually consistent.
2. **Given** browser tabs are visible, **When** I inspect page titles, **Then** each title starts with "Plantwise" and never displays "Touching_Grass".
3. **Given** I am on the unified landing page, **When** I inspect the browser tab, **Then** the title is "Plantwise - Watering Guide".
4. **Given** navigation is displayed, **When** I view menu labels, **Then** labels describe user-facing functions and include "Plant Search" for plant lookup.
5. **Given** I view the landing page, **When** the page loads, **Then** concise messaging appears with a single primary tagline and reduced promotional noise.
6. **Given** I am signed in with a saved preferred ZIP code, **When** the landing page loads, **Then** ZIP code input is pre-populated; otherwise it remains empty.

---

### Edge Cases

- What happens when a signed-in user has no saved preferred ZIP code? The ZIP field remains empty and editable.
- What happens when autocomplete text is shorter than three characters? No suggestions are shown and normal manual input remains available.
- How does the experience behave when no autocomplete matches exist? A clear no-results state is shown without blocking manual correction.
- How does the unified page behave for keyboard-only users? All form controls, suggestion selection, and calculation action remain fully keyboard accessible.
- What happens on narrow mobile screens? Input, suggestions, and results remain readable and actionable without horizontal scrolling.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a single landing page that includes ZIP input, plant search, and watering recommendation calculation initiation.
- **FR-002**: The system MUST display watering recommendations on the landing page after successful calculation without redirecting users to a separate workflow page.
- **FR-003**: The system MUST remove redundant duplicate user-input workflow entry points previously split between Home and Watering Guide.
- **FR-004**: The system MUST preserve the existing recommendation calculation logic and outputs for identical user inputs.
- **FR-005**: The system MUST preserve the existing authentication flow and signed-in user behavior.
- **FR-006**: For authenticated users with saved preferred ZIP code data, the system MUST pre-populate ZIP input on landing-page load.
- **FR-007**: For unauthenticated users, the system MUST keep landing-page ZIP input empty by default.
- **FR-008**: The system MUST provide plant search suggestions after at least three entered characters using case-insensitive contains matching.
- **FR-009**: The system MUST include both common name and scientific name in each plant suggestion.
- **FR-010**: The system MUST populate the existing watering-guide plant identifier when a suggestion is selected.
- **FR-011**: The system MUST show a user-friendly no-results message when plant search returns no matches.
- **FR-012**: The system MUST apply consistent Plantwise visual design patterns across user-facing pages, including layout, typography, spacing, card and header treatment, and button styling.
- **FR-013**: The system MUST update browser tab titles so all user-facing pages begin with "Plantwise".
- **FR-014**: The system MUST ensure no user-facing page title displays "Touching_Grass".
- **FR-015**: The system MUST use intuitive navigation labels that describe user-facing functionality, including "Plant Search" for plant lookup.
- **FR-016**: The landing page MUST display the tagline "Dynamic watering advice for your plants that knows the weather." as the single primary message.
- **FR-017**: The system MUST preserve existing profile-management capabilities for signed-in users.
- **FR-018**: The system MUST deliver responsive behavior for desktop and mobile layouts for the consolidated landing and search interactions.
- **FR-019**: The system MUST maintain accessible keyboard and form-control interaction behavior for landing-page inputs, suggestion lists, and calculation actions.
- **FR-020**: ZIP code entry and validation behavior MUST continue to support all United States ZIP codes and MUST NOT restrict users to California-only assumptions.
- **FR-021**: The system MUST display at most 10 autocomplete suggestions at one time.
- **FR-022**: The unified landing page browser title MUST be exactly "Plantwise - Watering Guide".

### Key Entities *(include if feature involves data)*

- **Preferred ZIP Setting**: User-associated saved ZIP value used to pre-populate landing-page input for authenticated users.
- **Plant Catalog Entry**: Plant record containing common name, scientific name, and existing identifier used by recommendation calculations.
- **Watering Recommendation Result**: User-facing recommendation output presented on the landing page after calculation.
- **Navigation Label and Page Title Metadata**: User-facing naming values shown in menus and browser tabs.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of successful recommendation calculations can be completed from landing-page load to result display without navigating to a second workflow page.
- **SC-002**: For authenticated users with saved preferred ZIP settings, ZIP input is pre-populated on landing-page load in at least 95% of observed sessions.
- **SC-003**: At least 90% of plant selection attempts using three or more entered characters end with a valid plant selection on first attempt.
- **SC-004**: 100% of user-facing pages display browser titles that start with "Plantwise" and exclude "Touching_Grass".
- **SC-005**: In usability verification, users can identify where to perform plant lookup from navigation labels within 5 seconds on first visit.

## Assumptions

- Existing user preference data already contains preferred ZIP code values for users who previously saved them.
- Existing plant dataset quality is sufficient to support name-based matching without schema changes.
- Existing recommendation outputs and domain calculation behavior remain authoritative and unchanged.
- Existing authentication and profile workflows remain in use and are not redesigned by this feature.
- Existing weather, persistence, and deployment architecture remain unchanged and available.