# Plants Page Contract

## Route and search

- **Path**: `/Plants`
- **Method**: GET
- **Query**: `SearchTerm` (optional text)
- **Form behavior**: Submitting the search navigates to `/Plants` with the entered term in the query string. Leading and trailing whitespace is ignored.
- **Empty query**: Show neither plant rows nor the no-match message.
- **Matching**: Partial, case-insensitive substring match against common name or scientific name.
- **Result limit and order**: At most 50 unique matching plants, in the order found in the supported plant database.

## Response content

Each result includes:

1. Common name
2. Scientific name
3. Water factor

For a non-empty query with no matching records, show exactly: `This plant is not in our supported plant database.`

The page also provides a separate user-facing unavailable-data error if the local plant catalog cannot be loaded; this condition is not reported as a no-match.

## Accessibility and navigation

- Include a visible label programmatically associated with the search field.
- Use semantic headings and a table with a caption and column headers for results.
- Keep no-result feedback visible and associated with the result area; use polite status semantics for any in-page status update.
- Include a Plants link in the shared navigation. Do not change the home-page content.

## Boundaries

Use the existing `plants.json` catalog through `IPlantRepository.SearchAsync`. This is a server-rendered page, not a new API. Do not add filters, saved lists, autocomplete, plant details, database storage, or external plant-data services.
