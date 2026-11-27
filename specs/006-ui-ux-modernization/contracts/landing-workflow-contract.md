# Contract: Unified Landing Workflow

## Scope

This contract defines the expected UI and handler interaction for the consolidated landing page watering guide flow.

## 1) Landing Page Render Contract

- **Route**: Unified landing page route (`/`)
- **Purpose**: Render ZIP input, plant search, suggestion UI, and recommendation display region in one workflow.
- **Required UI Elements**:
  - ZIP input control (supports all U.S. ZIP users)
  - Plant search input control
  - Suggestion dropdown/status region
  - Calculate action
  - Recommendation display region (in-page)
- **Authenticated Behavior**:
  - If user has saved preferred ZIP, ZIP input is pre-populated on load.
- **Unauthenticated Behavior**:
  - ZIP input starts empty.

## 2) Plant Suggestion Handler Contract

- **Interaction**: As user types in plant search (3+ characters), client requests suggestions.
- **Input Contract**:
  - `query` (string, required, trimmed)
  - Minimum length: 3 characters
- **Matching Contract**:
  - Case-insensitive contains match against supported plant data
  - Maximum 10 results returned
- **Output Contract**:
  - `suggestions` array with items:
	- `plantIdentifier`
	- `commonName`
	- `scientificName`
	- `displayLabel`
  - `noResultsMessage` when no matches are found
- **Error Contract**:
  - User-friendly fallback message when plant data cannot be queried

## 3) Recommendation Submission Contract

- **Interaction**: User selects Calculate from same landing page.
- **Input Contract**:
  - `zipCode` (required, valid 5-digit ZIP)
  - `plantIdentifier` or resolved plant input (required)
- **Behavior Contract**:
  - Uses existing recommendation calculation logic unchanged.
  - Displays result in-page without redirecting to a separate workflow page.
  - Preserves existing validation and unavailable-data messaging patterns.

## 4) Naming and Title Contract

- Browser title format for pages: `Plantwise - <Page Purpose>`
- Unified landing page title: `Plantwise - Watering Guide`
- Forbidden title token: `Touching_Grass`
- Navigation contract includes label: `Plant Search`

## 5) Accessibility and Responsive Contract

- Suggestion interaction is keyboard operable (focus, selection, dismissal).
- Validation and status messages are exposed via accessible semantics.
- Landing inputs, suggestions, and recommendation region remain usable on mobile and desktop breakpoints.