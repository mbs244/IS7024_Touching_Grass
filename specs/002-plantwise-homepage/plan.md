# Implementation Plan: PlantWise Homepage

**Branch**: `home_panditau` | **Date**: 2026-11-07 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-plantwise-homepage/spec.md`

## Summary

Create a dedicated PlantWise homepage at `/` using the existing Razor Pages web application, Bootstrap, HTML, CSS, and JavaScript conventions. Preserve the current watering form at `/Index` and hand off a validated homepage ZIP code through `/Index?zipCode={value}` so the existing form can prefill it. The Index integration is limited to explicit route ownership and GET-prefill support; its POST validation, plant lookup, calculation, TempData transfer, and `/Results` behavior remain unchanged and require teammate review.

## Technical Context

**Language/Version**: C# on .NET 10

**Primary Dependencies**: ASP.NET Core Razor Pages, Bootstrap, existing client-side JavaScript, and existing validation conventions

**Storage**: N/A for the homepage; existing JSON location and plant data remain owned by the watering workflow

**Testing**: Existing xUnit unit-test project, Razor Page/integration verification where available, build validation, and manual browser checks for routes, responsiveness, keyboard navigation, and accessibility

**Target Platform**: ASP.NET Core web application on supported desktop and mobile browsers

**Project Type**: Razor Pages web application

**Performance Goals**: Preserve the existing page response behavior; avoid new network requests, duplicated lookups, or unnecessary client-side work on the homepage

**Constraints**: Keep the homepage limited to UI, navigation, ZIP entry, and workflow handoff. Do not change the existing watering POST contract or Results behavior. ZIP values are untrusted input and must support all United States ZIP codes without California-only rules.

**Scale/Scope**: One new homepage route and its presentation assets, plus the smallest review-required integration changes needed to keep `/Index` as the existing watering workflow.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Code Quality and Clarity**: Pass. Use the existing .NET 10 Razor Pages architecture, shared abstractions, and clear page/model names. No duplicate ZIP lookup or watering logic is planned.
- **Testable Delivery**: Pass with a required route and handoff verification. Tests and manual checks cover `/`, `/Index`, valid/invalid ZIP input, prefill behavior, and unchanged `/Results` behavior.
- **Consistent User Experience**: Pass. Reuse existing validation terminology and form behavior while adding accessible homepage navigation, focus states, responsive layout, and clear handoff feedback.
- **Performance by Design**: Pass. The homepage adds no external calls and passes the ZIP value directly to the existing workflow rather than performing a second lookup.
- **Maintainable Simplicity**: Pass. The design adds one page and one explicit handoff contract, with no new persistence, API, or domain abstraction.
- **Engineering Constraints**: Pass. Input remains untrusted, existing location data owns support decisions, and the plan does not present recommendations or alter calculation behavior.
- **Review constraint**: Any existing Index-page change is explicitly limited, documented, and requires teammate review before implementation acceptance.

## Project Structure

### Documentation (this feature)

```text
specs/002-plantwise-homepage/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── homepage-workflow-handoff.md
└── checklists/
    └── requirements.md
```

`tasks.md` is intentionally not created by this planning stage; it belongs to the separate `/speckit-tasks` phase.

### Source Code (repository root)

```text
Touching_Grass/
├── Pages/
│   ├── Home.cshtml                 # New root homepage UI
│   ├── Home.cshtml.cs              # Only if homepage server-side state is needed
│   ├── Index.cshtml                # Existing watering form; route/prefill review only
│   ├── Index.cshtml.cs             # Existing workflow; additive GET binding review only
│   └── Shared/
│       └── _Layout.cshtml          # Navigation and shared page-shell review
├── wwwroot/
│   ├── css/site.css                # Homepage presentation and responsive styles
│   └── js/site.js                  # Only if a narrowly scoped interaction is required
└── tests/                          # Existing test project; add only relevant coverage
```

**Structure Decision**: Use the existing single Razor Pages web project and its existing `tests/Touching_Grass.UnitTests` project. Add the homepage under `Pages/`, reuse the existing layout and Bootstrap assets, and keep the watering workflow in its current `Pages/Index.cshtml` and `Pages/Index.cshtml.cs` files. No new project, API, storage layer, or workflow route is introduced.

## Implementation Sequence

1. Establish route ownership so the new homepage owns `/` and the existing watering page explicitly owns `/Index`; review this diff with the workflow teammate.
2. Build the homepage structure with the PlantWise logo, tagline, navigation, hero, ZIP form, Get Started action, and How It Works section.
3. Add responsive, plant-themed green/neutral styling using existing Bootstrap and site styles without reducing contrast or focus visibility.
4. Implement accessible ZIP validation and the `/Index?zipCode={value}` handoff while leaving existing POST validation and Results behavior unchanged.
5. Add or update focused tests for route ownership, ZIP handoff/prefill, invalid input feedback, and regression-sensitive existing workflow behavior where the project test seams support them.
6. Verify `/` renders the homepage and `/Index` renders the existing watering form without route conflicts before accepting the implementation.
7. Verify responsive behavior, keyboard navigation, accessibility feedback, and successful continuation through the unchanged `/Results` flow.

## Risks and Review Gates

- **Route conflict**: The current default Index page owns `/` and `/Index`; explicit route ownership must be verified before the homepage is accepted.
- **Teammate ownership**: Changes to `Index.cshtml` or `Index.cshtml.cs` must remain minimal and receive teammate review.
- **Workflow regression**: The existing POST validation, plant lookup, calculator, TempData key, redirect, and Results page are protected behaviors and must not be refactored as part of this feature.
- **ZIP coverage**: Homepage validation must accept five-digit ZIP formats throughout the United States and defer location support to the existing dataset/service; it must not add California-only restrictions.

## Complexity Tracking

No constitution violations are identified. The separate homepage page and explicit `/Index` route are the smallest safe structure that avoids replacing the existing workflow while allowing a server-visible ZIP handoff.
