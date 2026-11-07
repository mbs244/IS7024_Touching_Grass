# Research: Plant Search

## Search behavior and data access

- **Decision**: Add `SearchAsync(string searchTerm, CancellationToken cancellationToken = default)` to `IPlantRepository`. Trim the term; return an empty collection without loading the catalog when the term is empty. Otherwise, use `JsonDataLoader` to load `plants.json`, match partial substrings in either name using ordinal case-insensitive comparison, preserve catalog order, and return no more than the first 50 matches.
- **Rationale**: This reuses the established data source and repository boundary, is culture-independent for case-insensitive matching, and ensures both the result limit and empty-query behavior are consistently enforced for all callers.
- **Alternatives considered**: Filtering in the page model would duplicate repository logic and leave other callers without the same contract. Sorting or paging is not requested; preserving data order is deterministic and complies with the specification.

## Razor Pages search interaction

- **Decision**: Use a read-only GET form on `/Plants` and a query-bound page property with `SupportsGet = true`. Pass the handler cancellation token through `SearchAsync`.
- **Rationale**: Search is read-only, and GET makes a query reproducible by URL. Razor Pages does not bind page properties on GET unless `SupportsGet` is enabled. MVC supports cancellation-token binding from the request-aborted token, allowing asynchronous data loading to stop when the request is cancelled.
- **Alternatives considered**: POST would hide the search term from the URL despite no mutation. A handler parameter could bind from the query string, but the page property aligns with the form and can retain the entered value.
- **Sources**:
  - [Microsoft: Razor Pages and model binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0)
  - [Microsoft: Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-10.0)
  - [Microsoft: CancellationTokenModelBinder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.modelbinding.binders.cancellationtokenmodelbinder?view=aspnetcore-10.0)

## Accessibility and feedback

- **Decision**: Provide a programmatically associated visible label for the search input; use semantic headings and a captioned results table with clear column headers. Keep empty-result feedback visible and distinct from data-load errors. If feedback is updated without a page navigation, expose the concise status using a polite status region rather than an urgent alert.
- **Rationale**: This meets the requested basic accessibility while keeping normal search outcomes understandable. W3C status-message guidance applies to updates that do not change context or focus; a normal full-page GET navigation does not require a live region for that criterion. A polite live region is appropriate if the interaction later becomes an in-page update.
- **Alternatives considered**: A `role="alert"` for routine search feedback is unnecessarily interruptive. Making the whole results table a status message would announce too much content.
- **Sources**:
  - [W3C: Understanding WCAG 2.2 status messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages.html)
  - [Bootstrap: Alerts and accessibility](https://getbootstrap.com/docs/5.3/components/alerts/)

## Error behavior and tests

- **Decision**: Keep no-match feedback specific to a valid, non-empty search with zero matches. For local-data loading failures, follow the existing Razor Page pattern of logging the exception and displaying a separate user-friendly unavailable-data message. Add repository tests in the existing xUnit project, and smoke-test the rendered page using the existing app.
- **Rationale**: A missing or malformed required JSON file is not equivalent to a plant that is absent from the catalog. Existing local-data service tests already use a temporary `JsonDataLoader` fixture and require no additional packages.
- **Alternatives considered**: Returning an empty result for loader failures would incorrectly present an operational failure as a successful no-match search. Adding an HTTP test-host package conflicts with the no-new-packages constraint.
