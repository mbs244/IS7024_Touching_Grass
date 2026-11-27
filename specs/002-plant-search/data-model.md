# Data Model: Plant Search

## Existing entity: Supported plant

The feature searches and displays the existing plant records from `Touching_Grass/Data/plants.json`. It introduces no new persisted data.

| Field | Type | Use and validation |
|---|---|---|
| `Id` | Integer | Existing catalog identifier; not displayed by this page. |
| `CommonName` | String | Searched case-insensitively and displayed for every result. Existing data validation requires a non-empty value. |
| `ScientificName` | String | Searched case-insensitively and displayed for every result. Existing data validation requires a non-empty value. |
| `WaterFactor` | Decimal | Displayed for every result; existing data validation constrains it to 0 through 1 inclusive. |

## Search request

| Field | Type | Rules |
|---|---|---|
| `SearchTerm` | String | Bound from the GET query string; trim leading/trailing whitespace. An empty or whitespace-only value is not a search and produces no results or no-match message. |

## Search result

- A collection of distinct existing plant records matching the trimmed term as a substring of either `CommonName` or `ScientificName`, compared without regard to case.
- Preserve the order in `plants.json`; return at most the first 50 matching records.
- A non-empty search with no matching records produces the specified not-found feedback.
- A catalog load or validation failure is an unavailable-data error, not an empty search result.

## Page states

1. **Not searched**: no non-empty query; show the form without results or no-match feedback.
2. **Matches found**: show the result count and up to 50 matching plant rows.
3. **No matches**: show zero rows and `This plant is not in our supported plant database.`
4. **Data unavailable**: show a separate error if the required local catalog cannot be loaded or validated.
