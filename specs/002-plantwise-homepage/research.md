# Research: PlantWise Homepage

## Decision 1: Use a separate root homepage page

- **Decision**: Add a dedicated homepage page at `/` and keep the existing watering page at `/Index`.
- **Rationale**: The current `Pages/Index.cshtml` uses Razor Pages' implicit route and therefore owns both `/` and `/Index`. A separate homepage requires the existing page to use an explicit `/Index` route so the routes do not conflict. This preserves the existing workflow URL while allowing the homepage to own the root URL.
- **Alternatives considered**:
  - Replace the existing `Index` page with the homepage: rejected because it would risk removing or restructuring the teammate's watering workflow.
  - Move the workflow to a new route such as `/Watering`: rejected because it creates a larger route migration and is not necessary.
  - Use client-side storage or URL fragments for the handoff: rejected because it is less transparent, less server-testable, and would require workflow-side scripting.

## Decision 2: Use a ZIP query parameter for the handoff

- **Decision**: The homepage Get Started action navigates to `/Index?zipCode={five-digit-zip}`.
- **Rationale**: The existing workflow already owns ZIP validation and location lookup. A query parameter lets the homepage pass the value without duplicating lookup behavior or requiring the user to type the ZIP code again.
- **Alternatives considered**:
  - Submit the homepage form directly to the existing POST endpoint: rejected because the existing POST requires both ZIP code and plant name.
  - Add a new watering endpoint: rejected because it would duplicate or alter the existing workflow contract.

## Decision 3: Make only additive, review-required Index integration changes

- **Decision**: Plan the smallest integration change needed for the existing Index page to accept and display a ZIP code from the GET query string. Do not change its POST validation, plant lookup, calculation, TempData handling, or Results redirect.
- **Rationale**: `IndexModel.ZipCode` currently binds during POST only, while `IndexModel.OnPostAsync` already performs the required trim, five-digit validation, location lookup, plant lookup, calculation, and redirect to `/Results`. Enabling GET binding for the existing property preserves that behavior and avoids a second validation or lookup implementation.
- **Alternatives considered**:
  - Duplicate ZIP validation in the homepage and Index page: rejected because it creates two sources of truth.
  - Change the existing POST contract: rejected because it could affect teammate functionality and is outside the homepage responsibility.

## Decision 4: Verify routes before implementation is accepted

- **Decision**: Add a pre-acceptance verification that `/` renders the homepage and `/Index` renders the existing watering form with no route conflict.
- **Rationale**: Route ownership is the architectural risk introduced by separating the pages. Both paths must be verified before any visual or handoff work is considered complete.
- **Alternatives considered**: None; route verification is required by the feature request and constitution's testable-delivery principle.

## T004 Route Ownership Review

### Current routing configuration

- `Touching_Grass/Program.cs` registers Razor Pages with `AddRazorPages()` and maps them with `MapRazorPages()`; there are no custom route conventions or fallback endpoints that would override page-level routes.
- `Touching_Grass/Pages/Index.cshtml` currently declares `@page` without a route template. Razor Pages therefore exposes it at both `/` and `/Index`.
- `Touching_Grass/Pages/_ViewStart.cshtml` assigns the shared layout but does not affect route selection.

### Proposed before-and-after behavior

| Request | Current owner | Proposed owner | Expected behavior |
|---|---|---|---|
| `/` | `Pages/Index.cshtml` watering form | New `Pages/Home.cshtml` | PlantWise homepage renders |
| `/Index` | `Pages/Index.cshtml` watering form | `Pages/Index.cshtml` with explicit `/Index` route | Existing watering form renders |
| `/Index?zipCode=45236` | Existing form receives no GET-prefill value | Existing `/Index` form receives the query value after the separately reviewed GET-binding change | ZIP field is prefilled; plant input remains required |
| `/Results` | `Pages/Results.cshtml` | Unchanged `Pages/Results.cshtml` | Existing recommendation behavior remains available |

### Exact routing changes required in a later task

1. Change the existing `@page` directive in `Touching_Grass/Pages/Index.cshtml` to an explicit `/Index` route. This is an existing-workflow change and requires teammate review before implementation.
2. Add `Touching_Grass/Pages/Home.cshtml` with `@page "/"` for the new homepage. This creates no route conflict once the Index route is explicit.
3. Do not change `Touching_Grass/Program.cs`; the existing Razor Pages endpoint mapping is sufficient.
4. Verify `/` and `/Index` resolve to different pages before testing the ZIP handoff or changing shared navigation.

### T004 conclusion

The approach is technically sound under standard Razor Pages routing. The primary risk is accidental retention of the implicit Index root route, which would produce a route conflict or prevent the homepage from owning `/`. A secondary risk is changing shared navigation before route verification; navigation changes must remain a later, reviewed task. T004 is complete; no application files were changed.

## T005 Teammate Approval Record

Teammate approval was received for the minimal existing-workflow integration changes required by this plan:

1. Change `Touching_Grass/Pages/Index.cshtml` to use the explicit `/Index` route.
2. Add GET-based ZIP code prefill support in `Touching_Grass/Pages/Index.cshtml.cs`.

The approval does not authorize changes to POST validation, plant lookup, watering calculations, TempData, or Results behavior. Final teammate code review remains required before merging any implementation that changes either Index file.

## T006 Test Infrastructure Record

- Added the .NET 10-compatible `Microsoft.AspNetCore.TestHost` package only to `tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj`.
- Added `tests/Touching_Grass.UnitTests/RazorPagesTestHost.cs`, a test-only in-memory host that maps the real Razor Pages assembly and registers the existing PlantWise services without changing `Program.cs` or application files.
- Added `tests/Touching_Grass.UnitTests/RazorPagesTestHostTests.cs` with a smoke test proving the host can request `/Index` and observe the existing watering form, ZIP field, and plant field.
- Validation passed: the test project built successfully, the new smoke test passed, and all 9 tests in `Touching_Grass.UnitTests` passed.
- The host is ready for later route, homepage, ZIP-prefill, and Results regression tests. Those assertions are intentionally not added until the corresponding application tasks are approved.
- The test host duplicates the current service-registration list for isolation; if application registration changes later, the test fixture must be reviewed for parity.

## T007 Homepage ZIP Input and Handoff Decision

- **Decision**: Implement the homepage ZIP handoff with Razor markup and a native GET form; do not create `Home.cshtml.cs` and do not add JavaScript for the basic handoff.
- **Input shape**: Use a text-backed string field with a five-digit numeric constraint, `inputmode="numeric"`, postal-code autocomplete, and accessible label/help/error elements. A text value is required so ZIP codes such as `00501` retain their leading zeros.
- **Validation**: Use the form's HTML constraints for immediate user feedback on empty, nonnumeric, and non-five-digit values. The homepage must not perform location lookup or watering calculations. The existing `/Index` workflow remains authoritative for supported-location validation and all recommendation behavior.
- **Handoff**: Submit the form with `method="get"` to `/Index` using the field name `zipCode`. A valid value is therefore encoded by the browser as `/Index?zipCode={value}` without custom URL-building JavaScript.
- **State ownership**: The homepage owns only the transient input until navigation. The existing Index page owns the prefilled workflow field after the GET-binding integration approved in T005.
- **Progressive enhancement**: JavaScript is not required for the core handoff. If later UX work adds custom validation or focus behavior, it must enhance—not replace—the native form behavior and must not duplicate ZIP lookup or watering logic.
- **Scope boundary**: This decision does not implement `Home.cshtml`, `Home.cshtml.cs`, JavaScript, or the Index GET-binding change. Those belong to later approved tasks.

## T012 Page Model Verification

- `Touching_Grass/Pages/Home.cshtml` has no server-side state, page-specific data loading, or server-side handler requirements.
- The ZIP input is a string-backed HTML control with `pattern="[0-9]{5}"`, `minlength="5"`, `maxlength="5"`, and `required`, preserving values such as `00501`.
- The native GET form targets `/Index` and uses `name="zipCode"`, so the browser produces `/Index?zipCode={value}` without a separate page model or custom handoff logic.
- The homepage performs no ZIP lookup, plant lookup, watering calculation, or recommendation generation. The existing Index workflow remains responsible for authoritative location validation and recommendation behavior.
- `Home.cshtml.cs` is not necessary and was not created. T012 is complete.

## T013 Homepage-Owned Handoff Verification

- `Home.cshtml` uses `method="get"` and `action="/Index"` on the Get Started form.
- The ZIP control uses `name="zipCode"`, so native form submission creates `/Index?zipCode={value}`.
- The Get Started control is `type="submit"` and submits the containing form.
- The ZIP control uses `type="text"`, `pattern="[0-9]{5}"`, `minlength="5"`, and `maxlength="5"`, preserving five-character strings and leading zeros such as `00501`.
- The `required` and `pattern` constraints provide browser-side rejection during normal form interaction for empty or malformed values.
- Native browser form submission safely URL-encodes the query parameter; no custom URL-building JavaScript is needed.
- No JavaScript or separate page model is required for the homepage-owned handoff.
- End-to-end navigation and prefill cannot yet be verified because the existing implicit `Index` route still owns `/`, and the approved Index GET-prefill change is reserved for T014.
- No Home markup change was necessary. T013 is complete.

## Existing Workflow Findings

- `Pages/Index.cshtml` currently contains the ZIP and plant inputs and posts to `IndexModel.OnPostAsync`.
- `IndexModel.OnPostAsync` requires a five-digit ZIP code and a plant name, resolves the ZIP through `IZipLookupService`, calculates the recommendation, stores it in TempData, and redirects to `/Results`.
- `Pages/Results.cshtml.cs` reads the existing recommendation from TempData; this behavior is outside the homepage scope and must remain unchanged.
- `ZipLookupService` trims input, enforces five numeric digits, and resolves the value from the existing location data. The homepage must not replace this logic.
- Current shared navigation links point to `/Index` for Home and to `/Privacy`; they will require review during implementation so the homepage link and workflow link are clear without breaking existing destinations.

## T021 Responsive and Accessibility Verification

- Reviewed `Home.cshtml`, `_Layout.cshtml`, and `wwwroot/css/site.css` without modifying application code.
- Documented manual checks in `quickstart.md` for 375px mobile, 768px tablet, 1440px desktop, keyboard-only navigation, visible focus, ZIP validation feedback, 200% zoom, high contrast, horizontal overflow, and Bootstrap mobile navigation.
- No live browser session or device/assistive-technology validation was performed; all manual checks remain explicitly pending.
- Automated coverage confirms semantic headings and sections, ZIP label/help associations and constraints, meaningful navigation labels/order, and native GET submission. It does not claim to verify focus behavior, screen-reader announcements, high contrast, zoom, or visual overflow.
- Follow-up observations are recorded for T022–T025 in `quickstart.md`; no issue was expanded into those tasks.

## Final Verification Review

- T023: No homepage JavaScript is needed; native GET form submission and browser validation are sufficient, while existing `site.js` remains limited to the watering form.
- T024-T025: Static integration tests cover semantic structure, ZIP label/help association, native submit behavior, local image alt text, and navigation labels. Focus visibility, browser validation feedback, screen-reader behavior, zoom, high contrast, and responsive layout remain manual checks.
- T026-T028: Root, `/Index`, and ZIP-prefill route behavior are covered by existing Razor Pages integration tests.
- T029: Valid existing workflow POST regression is covered by `WorkflowRegressionIntegrationTests`.
- T030: ZIP contract and lookup ownership are automated; native browser invalid-submission behavior remains manual because TestHost does not execute HTML constraint validation.
