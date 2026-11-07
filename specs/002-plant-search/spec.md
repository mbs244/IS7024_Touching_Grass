# Feature Specification: Plant Search

**Feature Branch**: `002-plant-search`

**Created**: 2026-11-07

**Status**: Draft

**Input**: User description: "Add a /Plants page where users search plants by partial, case-insensitive common or scientific name and see all matching plants (common name, scientific name, water factor). Show "This plant is not in our supported plant database." when nothing matches. Constraints: reuse the existing plants.json data and IPlantRepository (add a SearchAsync method); don't change the existing home page. Out of scope: filters, saved plant lists, autocomplete, plant detail pages, API. Verification: Given "salvia", results include white sage; given "TOYON", toyon appears; given "sklujapouetllkjsda", zero results and the not-found message."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Find plants by name (Priority: P1)

A user opens the Plants page and searches for part or all of a plant's common or scientific name to find every matching supported plant and compare its names and water factor.

**Why this priority**: Finding supported plants is the primary purpose of the page.

**Independent Test**: Search for `salvia` and verify that white sage is among the results; search for `TOYON` and verify that toyon appears. Confirm each result shows its common name, scientific name, and water factor.

**Acceptance Scenarios**:

1. **Given** the supported plant database contains white sage, **When** the user searches for `salvia`, **Then** the matching results include white sage.
2. **Given** the supported plant database contains toyon, **When** the user searches for `TOYON`, **Then** toyon appears among the results.
3. **Given** a search term matches supported plants by common or scientific name, **When** the user searches for that term, **Then** up to 50 matching plants appear once each with their common name, scientific name, and water factor.

### User Story 2 - Understand when a plant is not found (Priority: P2)

A user receives a clear explanation when their search term does not match a supported plant.

**Why this priority**: Clear feedback helps users understand the limits of the supported plant database rather than mistaking an empty result for a page failure.

**Independent Test**: Search for `sklujapouetllkjsda` and verify that there are zero plant results and the specified not-found message is shown.

**Acceptance Scenarios**:

1. **Given** no supported plant name contains the searched term, **When** the user searches for `sklujapouetllkjsda`, **Then** no plant results appear and the page displays exactly: `This plant is not in our supported plant database.`
2. **Given** a search has matching results, **When** the user changes the search to a term with no matches, **Then** the previous results are replaced by the not-found message.

### Edge Cases

- A search term may partially match either the common name or scientific name, and matching is case-insensitive.
- Leading and trailing whitespace around a search term is ignored.
- A plant matching the term in both name fields appears only once.
- An empty search term does not return the entire catalog or display the no-match message.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST provide a Plants page where users can search the supported plant database.
- **FR-002**: The search MUST find partial matches in either a plant's common name or scientific name, without regard to letter case.
- **FR-003**: The page MUST display no more than 50 matching plants, including each displayed plant's common name, scientific name, and water factor. If more than 50 plants match, it MUST display the first 50 in the order they appear in the supported plant database.
- **FR-004**: When a non-empty search has no matches, the page MUST display exactly: `This plant is not in our supported plant database.`
- **FR-005**: The search MUST reuse the existing `plants.json` data through `IPlantRepository`, adding a `SearchAsync` search capability, and MUST NOT require an external plant-data service.
- **FR-006**: The existing home page MUST remain unchanged.
- **FR-007**: The feature MUST NOT add filters, saved plant lists, autocomplete, plant detail pages, or an API.
- **FR-008**: The page MUST treat an empty search as no search request: it MUST show no plant results and MUST NOT display the no-match message.
- **FR-009**: The search MUST ignore leading and trailing whitespace in a submitted search term.

### Key Entities

- **Supported plant**: A plant in the existing supported plant database, identified for display by its common name and scientific name and associated with a water factor.
- **Search term**: The user's text query used to find partial, case-insensitive matches in supported plant names.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In the specified acceptance checks, a search for `salvia` includes white sage, and a search for `TOYON` includes toyon.
- **SC-002**: Search results display no more than 50 plants, each with all three requested fields: common name, scientific name, and water factor.
- **SC-003**: A search for `sklujapouetllkjsda` returns zero plants and displays the exact specified not-found message.
- **SC-004**: Users can complete each of the matching and no-match search tasks from the Plants page without opening a plant detail page.

## Assumptions

- The existing supported plant database is the authoritative catalog for this feature; no plant data is added or fetched remotely.
- Search results are presented together on the Plants page and preserve the order of the supported plant database; at most 50 results are shown.
- An empty search is not a request to browse all plants; it leaves the result area empty without a no-match message.
- The existing `plants.json` data and `IPlantRepository` are retained; `IPlantRepository` gains the requested `SearchAsync` capability.
