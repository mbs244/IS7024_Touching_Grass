# Homepage-to-Workflow Contract

## Purpose

Define the smallest handoff from the new PlantWise homepage to the existing watering workflow without changing the workflow's POST or Results behavior.

## Route Ownership

| URL | Owner | Expected result |
|---|---|---|
| `/` | New homepage page | Renders PlantWise branding, hero content, ZIP input, Get Started action, and How It Works content |
| `/Index` | Existing watering page | Renders the existing ZIP and plant form |
| `/Index?zipCode={value}` | Existing watering page with homepage handoff | Renders the existing form with the supplied ZIP value prefilled; plant input remains available and required |
| `/Results` | Existing results page | Continues to display the existing recommendation after the unchanged workflow POST succeeds |

## Handoff Request

- **Method**: GET
- **Path**: `/Index`
- **Query parameter**: `zipCode`
- **Value**: The homepage's submitted five-digit ZIP string, preserving leading zeros.

## Expected Behavior

1. A valid homepage ZIP is encoded into the query parameter and sent to `/Index`.
2. The existing Index form displays the ZIP value so the user does not re-enter it.
3. The user supplies the plant name through the existing form.
4. The existing POST validation and watering calculation execute unchanged.
5. A successful POST continues to `/Results` using the existing behavior.

## Non-Goals

- The homepage does not submit a recommendation.
- The homepage does not supply a plant name.
- The handoff does not change the existing POST endpoint, validation messages, lookup service, calculator, TempData key, or Results page.
- No new API or persistence contract is introduced.

## Review Gate

Any change to `Pages/Index.cshtml` or `Pages/Index.cshtml.cs` must be limited to route ownership and GET-prefill support, explicitly reviewed by the teammate responsible for the watering workflow, and verified not to alter POST or Results behavior.
