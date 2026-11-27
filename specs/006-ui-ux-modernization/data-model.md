# Data Model: UI Consolidation and User Experience Modernization

## Overview

This feature does not add database tables or alter existing schema. It introduces UI interaction models and contract-level view data used by existing Razor Pages and services.

## Entities

## 1) Landing Watering Input

- **Purpose**: Captures user-provided ZIP and plant selection on the unified landing workflow.
- **Fields**:
  - `ZipCode` (string, required, 5 numeric characters, USA ZIP semantics)
  - `PlantName` (string, required, trimmed user input)
  - `PlantIdentifier` (string, required after autocomplete selection; maps to existing plant identity used by recommendations)
- **Validation Rules**:
  - ZIP must remain valid for all U.S. ZIP users (no California-only constraint).
  - Plant input must resolve to a supported plant or provide user-friendly feedback.
- **State Transitions**:
  - `Empty` → `Editing` → (`ValidationError` | `ReadyToCalculate`) → (`RecommendationDisplayed` | `RequestError`)

## 2) Plant Suggestion Item

- **Purpose**: Represents a single autocomplete candidate in dropdown UI.
- **Fields**:
  - `PlantIdentifier` (string; canonical value used by existing recommendation lookup)
  - `CommonName` (string)
  - `ScientificName` (string)
  - `DisplayLabel` (string; combined common/scientific presentation)
- **Validation Rules**:
  - Returned only when query length is at least 3 characters.
  - Matching rule is case-insensitive contains.
  - Result set is capped at 10 suggestions.
- **State Transitions**:
  - `Hidden` → `Loading` → (`SuggestionsVisible` | `NoResults`) → `Selected` or back to `Editing`

## 3) Watering Recommendation View Data

- **Purpose**: Displays the computed recommendation and explanatory details in the unified landing flow.
- **Fields**:
  - `PlantDisplayName`
  - `ZipCode`
  - `NetNeedInches`
  - `Explanation`
  - `EvapotranspirationInches`
  - `PlantFactor`
  - `GrossNeedInches`
  - `RainfallInches`
  - `WaterStatusMessage` (derived message)
- **Validation Rules**:
  - Values are sourced from existing recommendation calculation output.
  - Display formatting must preserve explicit units and current precision style.
- **State Transitions**:
  - `NotAvailable` → (`Available` | `DisplayError`) after calculation attempt

## 4) Page Metadata Model

- **Purpose**: Defines user-facing naming consistency across navigation and browser tabs.
- **Fields**:
  - `PageTitle` (must begin with `Plantwise`)
  - `NavigationLabel` (user-facing functional label)
- **Validation Rules**:
  - Unified landing title is exactly `Plantwise - Watering Guide`.
  - No page title may include `Touching_Grass`.
  - Navigation includes `Plant Search` label for plant lookup.

## Relationships

- `Landing Watering Input` selects one `Plant Suggestion Item` (or manually resolves equivalent identifier).
- `Landing Watering Input` produces one `Watering Recommendation View Data` result per successful calculation.
- `Page Metadata Model` applies across all rendered pages and navigation components.

## Non-Persistent Data Notes

- Preferred ZIP autofill is read from existing authenticated user profile data.
- No new persisted entities are introduced.