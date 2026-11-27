# Phase 0 Research: UI Consolidation and User Experience Modernization

## Decision 1: Landing-page workflow consolidation

- **Decision**: Use the current root route (`/`) as the single user-facing landing and watering workflow entry and fold the existing watering input experience into that page.
- **Rationale**: The spec requires one landing experience and no backend architecture change. Consolidating at the page layer avoids service churn and keeps existing calculation/authentication behavior intact.
- **Alternatives considered**:
  - Keep separate Home and Watering Guide pages with stronger linking — rejected because it preserves redundant workflow.
  - Keep current root page and move all interactions to `/Index` only — rejected because root must become the unified landing experience.

## Decision 2: Recommendation display strategy

- **Decision**: Display watering recommendation results within the unified landing flow instead of sending users through a separate workflow page.
- **Rationale**: Direct on-page display satisfies FR-002 and improves usability by reducing context switching.
- **Alternatives considered**:
  - Continue redirecting to `Results` page — rejected because it violates the unified in-place result requirement.
  - Open results in modal overlay — rejected as unnecessary complexity for a server-rendered Razor Pages flow.

## Decision 3: Autocomplete matching and limits

- **Decision**: Implement plant suggestion matching as case-insensitive contains with a maximum of 10 results after 3+ characters.
- **Rationale**: These values are explicitly clarified in the spec and balance discoverability with bounded payload/visual noise.
- **Alternatives considered**:
  - Prefix-only matching — rejected due to lower findability for users typing partial interior terms.
  - Unbounded results — rejected for performance/readability concerns.

## Decision 4: Contract shape for suggestions

- **Decision**: Add a Razor Page handler contract for suggestions that returns structured suggestion payload including display labels and existing plant identifier mapping.
- **Rationale**: Keeps implementation within existing Razor Pages conventions while enabling responsive client-side suggestions without introducing a new backend service.
- **Alternatives considered**:
  - Full-page postback suggestion flow — rejected due to poor UX for autocomplete.
  - New standalone API controller — rejected as unnecessary architectural expansion.

## Decision 5: Browser title and navigation standardization

- **Decision**: Standardize page titles to `Plantwise - <Page Purpose>` format and update navigation labels to user-facing names (including `Plant Search`).
- **Rationale**: Required by FR-013 through FR-015 and SC-004/SC-005; improves clarity and removes internal naming exposure.
- **Alternatives considered**:
  - Keep mixed historical title formats — rejected due to inconsistent user experience.

## Decision 6: Visual design consistency approach

- **Decision**: Use existing PlantWise design language from the current home experience as the baseline and apply it to key pages via shared CSS patterns.
- **Rationale**: Satisfies consistency requirements without introducing new UI frameworks or redesigning backend flows.
- **Alternatives considered**:
  - Full visual redesign from scratch — rejected as out of scope and higher risk.
  - Keep page-specific styling differences — rejected due to cohesion goals.

## Decision 7: Test strategy for constitution compliance

- **Decision**: Expand Razor Pages host tests and targeted page-behavior tests for unified flow rendering, title/nav text, and suggestion/result interaction states.
- **Rationale**: Meets constitution requirements for testable delivery while staying aligned to current xUnit/TestHost setup.
- **Alternatives considered**:
  - Manual verification only — rejected due to regression risk.
  - End-to-end browser automation only — rejected as unnecessary for this increment and current test stack.