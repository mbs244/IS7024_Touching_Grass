---

description: "Actionable implementation tasks for the PlantWise homepage"
---

# Tasks: PlantWise Homepage

**Input**: Design documents from `/specs/002-plantwise-homepage/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/homepage-workflow-handoff.md`, and `quickstart.md`

**Scope**: Homepage UI and its minimal integration with the existing watering workflow. No watering-calculation, plant-selection, weather, or Results redesign is included.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel when tasks use different files and have no unmet dependency.
- **[Story]**: Maps the task to a user story from `spec.md`.
- Every task names the expected file or verification target.

## Phase 1: Setup

**Purpose**: Confirm the implementation boundary and existing project conventions before editing application code.

- [X] T001 Review `specs/002-plantwise-homepage/spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/homepage-workflow-handoff.md`, and `quickstart.md` before implementation.
- [X] T002 [P] Confirm the existing Razor Pages project targets .NET 10 and uses Bootstrap, `Pages/`, `wwwroot/css/site.css`, `wwwroot/js/site.js`, and the existing xUnit project; record any unexpected deviation before implementation.
- [X] T003 [P] Confirm the existing workflow contract in `Touching_Grass/Pages/Index.cshtml.cs`, `Touching_Grass/Services/IZipLookupService.cs`, and `Touching_Grass/Pages/Results.cshtml.cs`; do not change these files during setup.

---

## Phase 2: Foundational Route and Integration Prerequisites

**Purpose**: Establish the route and handoff boundaries that all homepage stories depend on.

**CRITICAL**: The existing `/Index` workflow must remain available before homepage work is accepted.

- [X] T004 Define the route ownership change so the new homepage owns `/` and the existing watering page explicitly owns `/Index`; document the before/after route behavior in the implementation review.
- [X] T005 Obtain teammate review approval before making any change to `Touching_Grass/Pages/Index.cshtml` or `Touching_Grass/Pages/Index.cshtml.cs`; approval is limited to explicit `/Index` route ownership and GET-based ZIP prefill support, with final teammate code review required before merge.
- [X] T006 Add or configure the smallest test seam needed to exercise Razor Page route resolution and the `/Index?zipCode={value}` handoff without changing production workflow behavior; keep any test dependency addition limited to the existing test project. The in-memory host builds and the existing 9-test project suite passes.
- [X] T007 [P] Define the homepage ZIP input and handoff behavior without a separate page model: use a string-backed native GET form to `/Index` with `name="zipCode"`, five-digit numeric constraints, preserved leading zeros, and no duplicate lookup or watering-calculation logic.

**Checkpoint**: Route ownership, teammate review boundary, and the test approach are agreed before user-story implementation begins.

---

## Phase 3: User Story 1 - Discover PlantWise and Begin a Lookup (Priority: P1) MVP

**Goal**: Deliver a recognizable PlantWise homepage that explains the product and starts the existing workflow with no ZIP re-entry.

**Independent Test**: Open `/`, confirm the brand and hero value proposition, submit a valid five-digit United States ZIP code through Get Started, and confirm `/Index?zipCode={value}` displays the existing watering form with the ZIP prefilled.

### Tests for User Story 1

- [X] T008 [P] [US1] Add homepage route and content coverage for `/` in the existing test project, verifying the homepage renders its required branding and hero content. The test compiles and reaches the existing root page, then fails as expected because the homepage tagline and hero content are not implemented.
- [X] T009 [P] [US1] Add ZIP handoff coverage for `/Index?zipCode=45236`, verifying the existing watering form receives and displays the value without changing the existing POST contract.
- [ ] T010 [P] [US1] Add invalid homepage ZIP coverage for empty, malformed, and unsupported-location inputs, verifying clear feedback and no recommendation is submitted from the homepage.

### Implementation for User Story 1

- [X] T011 [P] [US1] Create `Touching_Grass/Pages/Home.cshtml` with the homepage structure, semantic headings, logo placement, exact tagline `Water smarter. Grow better.`, hero explanation, ZIP input, and Get Started action.
- [X] T012 [US1] Verify whether `Touching_Grass/Pages/Home.cshtml.cs` is required for homepage-bound ZIP state or validation; no page model is necessary because the native string-backed GET form handles the transient ZIP handoff without duplicating location lookup or watering-calculation logic.
- [X] T013 [US1] Verify the Get Started handoff in `Touching_Grass/Pages/Home.cshtml`: the native string-backed GET form targets `/Index` with `name="zipCode"`, browser-side five-digit constraints, preserved leading zeros, and no separate model or script. End-to-end navigation remains deferred until the T014 route and GET-prefill changes.
- [X] T014 [US1] After teammate approval, apply only the approved route and GET-prefill changes to `Touching_Grass/Pages/Index.cshtml` and/or `Touching_Grass/Pages/Index.cshtml.cs`; do not alter POST validation, plant lookup, calculator invocation, TempData, or `/Results` redirect behavior.
- [X] T015 [US1] Update `Touching_Grass/Pages/Shared/_Layout.cshtml` only as needed to expose clear homepage and existing-page navigation destinations; preserve the `/Index`, `/Privacy`, and existing footer behavior unless reviewed.

**Checkpoint**: A visitor can identify PlantWise, submit a valid nationwide ZIP, reach `/Index`, and continue using the existing watering form.

---

## Phase 4: User Story 2 - Understand How PlantWise Works (Priority: P1)

**Goal**: Explain the three-step PlantWise journey and provide clear navigation to existing application pages.

**Independent Test**: Read the homepage without implementation knowledge and verify the ordered steps are present and understandable; activate navigation links and confirm their existing destinations open.

### Tests for User Story 2

- [X] T016 [P] [US2] Add content and navigation coverage for the How It Works section and existing destination links, verifying the ordered labels Enter Your Location, Choose Your Plants, and Get Watering Guidance.

### Implementation for User Story 2

- [X] T017 [P] [US2] Add the How It Works section to `Touching_Grass/Pages/Home.cshtml` with the required ordered steps and concise user-focused explanations.
- [X] T018 [P] [US2] Add or refine homepage navigation in `Touching_Grass/Pages/Home.cshtml` and, if required, `Touching_Grass/Pages/Shared/_Layout.cshtml` so link text identifies destinations and does not redirect the watering workflow unexpectedly.
- [X] T019 [US2] Keep homepage scope limited to presentation, navigation, ZIP entry, and handoff; do not add plant-selection, weather, recommendation, or Results behavior to `Touching_Grass/Pages/Home.cshtml`.

**Checkpoint**: Visitors understand the three-step journey and can reach existing pages without changing their behavior.

---

## Phase 5: User Story 3 - Use the Homepage on Any Device (Priority: P1)

**Goal**: Make the homepage responsive, accessible, and easy to navigate across viewport sizes and input methods.

**Independent Test**: Use the homepage at narrow, medium, and wide widths with keyboard-only navigation and verify all required content and controls remain usable.

### Tests for User Story 3

- [X] T020 [P] [US3] Add or extend UI/integration coverage for semantic headings, labeled ZIP input, validation association, navigation order, and visible focus behavior using the project's available test approach.
- [X] T021 [P] [US3] Add responsive and accessibility verification coverage or documented browser checks for phone, tablet, desktop, keyboard-only use, increased text size, high contrast, and no horizontal scrolling.

### Implementation for User Story 3

- [X] T022 [P] [US3] Add homepage-specific responsive and plant-themed green/neutral styles to `Touching_Grass/wwwroot/css/site.css`, preserving readable contrast and existing shared styles.
- [X] T023 [P] [US3] Add only narrowly scoped homepage interaction behavior to `Touching_Grass/wwwroot/js/site.js` or a homepage-owned script if needed; preserve existing submit-spinner behavior and avoid workflow changes. No homepage-specific JavaScript is required because native form behavior is sufficient.
- [X] T024 [US3] Ensure `Touching_Grass/Pages/Home.cshtml` uses accessible labels, meaningful link text, semantic sections/headings, validation messaging associated with the ZIP control, keyboard-operable controls, and visible focus states. Static markup and CSS/test checks pass; live keyboard and browser-validation checks remain manual.
- [X] T025 [US3] Verify decorative logo or plant imagery has appropriate alternative treatment and does not obscure the product name or tagline for assistive-technology users. Static markup checks pass; assistive-technology review remains manual.

**Checkpoint**: The homepage remains readable, responsive, keyboard-operable, and understandable without changing the existing watering workflow.

---

## Phase 6: Route, Workflow, and Cross-Cutting Verification

**Purpose**: Validate the complete feature and protect teammate-owned behavior before implementation acceptance.

- [X] T026 Verify `/` renders the new PlantWise homepage and does not resolve to the existing watering form.
- [X] T027 Verify `/Index` renders the existing watering form with both ZIP code and plant inputs and no route conflict with `/`.
- [X] T028 Verify `/Index?zipCode=45236` pre-fills the existing ZIP field, preserves the plant field requirement, and does not submit a recommendation by itself.
- [X] T029 Verify a valid ZIP and plant submitted through the existing form still use the unchanged workflow and reach `/Results` with existing recommendation behavior.
- [ ] T030 Verify malformed, empty, and unsupported ZIP scenarios retain understandable validation or unavailable-location behavior and do not bypass existing validation.
- [ ] T031 Run the applicable automated tests, build, and the steps in `specs/002-plantwise-homepage/quickstart.md`; resolve regressions without expanding scope.
- [ ] T032 Obtain final teammate review of every diff touching `Touching_Grass/Pages/Index.cshtml` or `Touching_Grass/Pages/Index.cshtml.cs`, confirming only route ownership and GET-prefill support changed.
- [ ] T033 Confirm no changes were made to watering calculation, plant selection, weather behavior, Results behavior, or unrelated teammate features.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No implementation dependencies; establishes scope and confirms existing contracts.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks homepage implementation until route ownership, teammate review, and test seams are defined.
- **Phase 3 (User Story 1)**: Depends on Phase 2; delivers the MVP homepage and ZIP handoff.
- **Phase 4 (User Story 2)**: Depends on the homepage structure from Phase 3, but content and navigation can be developed in parallel after `T011` is available.
- **Phase 5 (User Story 3)**: Depends on the homepage markup from Phases 3 and 4; styling and accessibility checks can proceed in parallel where files do not conflict.
- **Phase 6 (Verification)**: Depends on all required story work and teammate review; must pass before implementation acceptance.

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational tasks; MVP.
- **US2 (P1)**: Depends on the homepage page created by US1; does not change the watering workflow.
- **US3 (P1)**: Depends on the homepage markup from US1 and US2; does not change domain behavior.

### Parallel Opportunities

- `T008`, `T009`, and `T010` can run in parallel after the test seam is available.
- `T011` and `T017` can be split only if the homepage file ownership is coordinated; otherwise complete them sequentially to avoid conflicts.
- `T016`, `T022`, and `T023` can run in parallel when they target separate test, CSS, and JavaScript files.
- `T026` through `T030` can be performed as a verification pass after implementation, with route checks completed before workflow regression checks.

---

## Implementation Strategy

### MVP First

1. Complete setup and foundational route review.
2. Implement US1 homepage branding, ZIP input, Get Started handoff, and approved minimal Index integration.
3. Verify `/`, `/Index`, and `/Index?zipCode=...` before expanding the presentation.
4. Add How It Works content and navigation.
5. Add responsive and accessibility refinement.
6. Run the full route, workflow, Results, test, build, and teammate-review gates.

### Incremental Delivery

- **Increment 1**: Root homepage with branding and ZIP handoff.
- **Increment 2**: How It Works content and clear navigation.
- **Increment 3**: Responsive, accessible presentation and final regression verification.

No task in this list authorizes implementation outside the homepage or the explicitly reviewed route/prefill integration boundary.

## Notes

- `[P]` tasks may run in parallel only when they use different files and have no unmet dependency.
- Any Index-page task is explicitly review-gated because the file belongs to the existing watering workflow.
- Do not create or switch Git branches; remain on `home_panditau`.
- Do not create additional features, endpoints, persistence, or recommendation logic as part of this task list.
