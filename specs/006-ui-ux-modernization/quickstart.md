# Quickstart: UI Consolidation and User Experience Modernization

## Goal

Validate the unified landing watering workflow, autocomplete behavior, and Plantwise branding/title consistency in local development.

## Prerequisites

- .NET 10 SDK installed
- Repository restored/buildable
- Default local development configuration (in-memory identity + JSON-backed plant/ZIP data)

## Run Locally

1. Build the solution.
2. Run the Razor Pages app project (`Touching_Grass`).
3. Open the application root route in browser.

## Manual Validation Flow

1. Confirm landing page title is `Plantwise - Watering Guide`.
2. Confirm landing page contains ZIP entry, plant search, and Calculate action in one view.
3. If authenticated with saved ZIP, confirm ZIP is pre-populated; if not authenticated, confirm field is empty.
4. Enter at least 3 characters in plant search and confirm:
   - suggestions appear,
   - matching is case-insensitive contains,
   - each suggestion shows common + scientific name,
   - list shows at most 10 results.
5. Select a suggestion and calculate watering recommendation.
6. Confirm recommendation appears in the landing workflow without redirecting to another workflow page.
7. Enter a non-matching plant query and confirm a user-friendly no-results message.
8. Navigate through pages and confirm:
   - titles start with `Plantwise`,
   - `Touching_Grass` is not shown in user-facing titles,
   - navigation includes `Plant Search` label,
   - visual style is consistent.

## Automated Test Focus

- Page host tests for unified landing form and in-page recommendation rendering.
- Tests for title and navigation label consistency.
- Tests for suggestion query rules (3+ chars, contains match, max 10, no-results state).
- Regression tests asserting recommendation calculation outputs remain unchanged for identical inputs.