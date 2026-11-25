# Data Model: PlantWise Homepage

## Scope

This feature introduces no new persisted data, domain entities, or database schema. The homepage collects a ZIP code only to hand it to the existing watering workflow.

## Homepage Input State

| Field | Type/Shape | Required | Rules | Source | Destination |
|---|---|---:|---|---|---|
| ZIP code | Five-character numeric string | Yes to start lookup | Exactly five digits; United States ZIP codes are valid regardless of state; trim surrounding whitespace before validation | Homepage visitor | Existing `/Index` workflow query parameter |

The value must remain a string so leading-zero ZIP codes are preserved.

### T007 Input and Handoff Representation

The planned homepage control is a text-backed string input rather than a numeric input.
The implementation should apply a five-digit numeric pattern, numeric input mode, and
postal-code autocomplete while retaining the submitted value as text. The homepage form
should use a native GET submission to `/Index` with `name="zipCode"`, producing
`/Index?zipCode={value}` without custom URL construction or a separate page model.

Native form constraints provide format feedback, but the existing watering workflow remains
the authoritative owner of location support, ZIP lookup, plant selection, calculations, and
recommendations. No duplicate lookup or calculation logic belongs in the homepage.

## Workflow Handoff State

| Field | Transport | Purpose | Ownership |
|---|---|---|---|
| `zipCode` | Query parameter on `/Index` | Prefill the existing watering form without requiring ZIP re-entry | Homepage creates it; existing Index page reads it |

The handoff is not a new domain object and must not trigger a recommendation by itself because the existing workflow still requires plant input.

## Existing Index Workflow State (Unchanged)

The existing workflow continues to own:

- POST-bound ZIP code and plant name validation.
- ZIP location resolution through the existing lookup service.
- Plant matching and ambiguity handling.
- Watering calculation and recommendation creation.
- TempData transfer and `/Results` redirect.

The plan does not introduce new fields or change the existing POST contract.

## Validation and Error States

- Empty homepage ZIP: explain that a five-digit ZIP code is required; remain on the homepage.
- Malformed homepage ZIP: explain the five-digit numeric requirement; remain on the homepage.
- Valid-format but unsupported ZIP: use the existing location availability behavior where applicable; do not fabricate a location.
- Invalid or missing `zipCode` query value at `/Index`: preserve the existing form's correction and validation behavior; do not bypass the existing POST validation.

## Data Protection and Scope

The ZIP code is untrusted user input. The implementation must HTML-encode or framework-bind it through existing safe mechanisms and must not log or persist it as a new record. No plant, weather, recommendation, or user-account data is added by this feature.
