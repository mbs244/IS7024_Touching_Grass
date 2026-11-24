# Feature Specification: Remove Plants from My Plants

**Feature Branch**: `004-remove-plants`

**Created**: 2026-11-23

**Status**: Draft

**Input**: User description: "I want users to be able to remove plants from their My Plants list. Right now you can only favorite them. Add a Remove button next to each plant. When you click it the plant should go away and show a message like 'Toyon was removed from your plants.' Users should only be able to remove their own plants. Only change the My Plants page, not the home page. It needs to work both locally and on Azure so it should go in both plant services, kind of like how the favorite toggle works. Don't change the database or add migrations. The button should say which plant it removes for accessibility."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Remove a plant from My Plants (Priority: P1)

As a user, I want to remove a plant from my personal plant list so that I can keep that list up to date.

**Why this priority**: Removing unwanted plants is the central value of this feature.

**Independent Test**: Add a plant to a user's list, remove it from My Plants, and verify it no longer appears in that user's list while remaining available elsewhere.

**Acceptance Scenarios**:

1. **Given** a user has a plant in My Plants, **When** they activate that plant's Remove button, **Then** the plant disappears from their list and a confirmation identifies the plant by name, such as "Toyon was removed from your plants."
2. **Given** two users have plant lists, **When** one user removes a plant from their own list, **Then** the other user's list is unchanged.
3. **Given** a plant is shown on the home page, **When** a user removes a plant from My Plants, **Then** the home page's plant listing and behavior remain unchanged.

### User Story 2 - Identify and safely use removal controls (Priority: P2)

As a user, I want each removal control to identify its plant and to receive clear feedback if removal cannot be completed, so that I can act confidently and understand the result.

**Why this priority**: A clearly identified control prevents accidental removal, and truthful feedback makes failures understandable.

**Independent Test**: Inspect each removal control's accessible name and attempt a removal that cannot be completed; verify the plant remains listed and an error is communicated.

**Acceptance Scenarios**:

1. **Given** a plant is listed in My Plants, **When** its removal control is presented, **Then** the control's accessible name includes the plant's name and makes clear that it removes that plant.
2. **Given** a removal cannot be completed, **When** the user attempts it, **Then** the plant remains in the list and the user receives an understandable failure message.
3. **Given** a user attempts to remove a plant belonging only to another user, **When** the request is handled, **Then** the other user's plant remains unchanged and is not removed from that user's list.

### Edge Cases

- The user's list is empty: no plant-specific removal controls are shown.
- A plant was already removed or is no longer available when a stale removal is attempted: no other user's plant is affected, and the user receives accurate feedback.
- A removal fails: the plant remains visible in My Plants and no success confirmation is shown.
- The same plant is present on the home page: removing it from My Plants does not remove or alter its home-page listing.
- The same removal behavior is available in both locally run and Azure-hosted deployments.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The My Plants page MUST provide a Remove control for each plant in the current user's list.
- **FR-002**: Activating a Remove control MUST remove only that plant from the current user's list.
- **FR-003**: The system MUST prevent a user from removing a plant from another user's list.
- **FR-004**: After a successful removal, the My Plants page MUST show a confirmation that identifies the removed plant by name.
- **FR-005**: Each Remove control MUST have an accessible name that clearly indicates the action and identifies the plant it removes.
- **FR-006**: Plant removal MUST be limited to the My Plants experience and MUST NOT change the home page's plant listings or behavior.
- **FR-007**: Plant removal MUST behave consistently in locally run and Azure-hosted deployments.
- **FR-008**: If removal fails, the system MUST keep the plant in the user's list and communicate that removal did not succeed.
- **FR-009**: Removing a plant from a user's list MUST NOT remove the plant from other users' lists or from the available plant collection.

### Key Entities *(include if data involved)*

- **User's plant list**: The plants associated with one user for display in My Plants.
- **Plant**: A plant shown in a user's list and identified by a name for confirmation and accessible controls.
- **User**: The account whose own list may be changed; removal authority is limited to that user's list.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In all successful removal scenarios, one activation removes the selected plant from the user's My Plants list and displays a confirmation naming that plant.
- **SC-002**: In all cross-user removal attempts, the other user's list remains unchanged.
- **SC-003**: Every rendered removal control exposes an accessible name that identifies both the removal action and the plant.
- **SC-004**: All removal acceptance scenarios pass in both locally run and Azure-hosted deployments.
- **SC-005**: No home-page plant listing or behavior changes as a result of a My Plants removal.

## Assumptions

- The user is authenticated, and the application can identify which plant-list entries belong to that user.
- Existing plant-list data and ownership associations are sufficient to support removal; the feature does not require new plant data or a change to the user's broader plant collection.
- Feedback is presented on the My Plants page after the removal attempt.
- Failure feedback uses the application's existing user-facing error presentation conventions.
