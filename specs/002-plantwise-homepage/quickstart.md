# Quickstart: PlantWise Homepage Planning Verification

## Prerequisites

- .NET 10 SDK and the existing solution are available.
- The application is run from the repository root using the existing project configuration.
- No Git branch is created or switched; work remains on `home_panditau`.

## Required Route Verification

1. Start the existing Razor Pages application.
2. Open `/`.
   - Confirm the PlantWise homepage renders.
   - Confirm the logo, tagline, hero content, ZIP input, Get Started action, navigation, and How It Works section are present.
3. Open `/Index` directly.
   - Confirm the existing watering form renders.
   - Confirm both ZIP code and plant name inputs remain present.
4. Open `/Index?zipCode=45236`.
   - Confirm the existing watering form renders with `45236` prefilled.
   - Confirm the plant field remains available and required.
5. Submit the existing form with a valid ZIP and plant.
   - Confirm the existing workflow reaches `/Results`.
   - Confirm the recommendation and existing Results behavior are unchanged.
6. Repeat with a malformed ZIP and an unsupported location.
   - Confirm existing validation and unavailable-location behavior remain intact.

## Responsive and Accessibility Verification

- Check narrow, medium, and wide viewport sizes.
- Navigate the homepage and its form using only the keyboard.
- Confirm visible focus, logical order, usable validation feedback, and no keyboard trap.
- Confirm the navigation exposes clear destinations for the homepage and existing application pages.

## T021 Browser Verification Checklist

The following checks are required for manual browser verification. They were documented during T021 but were not executed by the automated test host or by a live browser session; each remains pending until manual verification is performed.

| Check | Viewport or method | Status | Notes |
|---|---|---|---|
| Mobile layout | 375px wide | Pending manual check | Confirm content remains readable, stacked, and usable. |
| Tablet layout | 768px wide | Pending manual check | Confirm the responsive layout remains usable at the Bootstrap breakpoint. |
| Desktop layout | 1440px wide | Pending manual check | Confirm content is readable without excessive empty space or overlap. |
| Keyboard navigation | Tab, Shift+Tab, Enter | Pending manual check | Confirm logical traversal through navigation, ZIP input, and Get Started. |
| Focus visibility | Keyboard interaction | Pending manual check | Confirm focused links, input, button, and menu toggle are visibly indicated. |
| ZIP accessibility | Label and invalid submission | Pending manual check | Confirm the ZIP label, browser validation, help text, and correction flow are understandable. |
| Increased text size | 200% browser zoom | Pending manual check | Confirm text and controls remain visible and usable without clipping. |
| High contrast | OS or browser high-contrast setting | Pending manual check | Confirm brand, text, controls, borders, and focus states remain distinguishable. |
| Overflow | All listed widths and 200% zoom | Pending manual check | Confirm no horizontal scrolling or overlapping content. |
| Mobile navigation | 375px Bootstrap navbar | Pending manual check | Confirm the menu toggle opens and closes the navigation and links remain usable. |

Automated Razor Pages tests cover semantic markup, labels, validation attributes, native submission, navigation labels, and route responses; they do not claim to verify the manual checks above.

T023 conclusion: no homepage-specific JavaScript is required. The homepage uses native HTML GET submission and browser constraint validation; the existing `site.js` remains scoped to the watering form's submit indicator.

T026-T030 automated evidence: the homepage, `/Index`, `/Index?zipCode=45236`, valid existing workflow POST, and existing malformed/unsupported lookup behavior are covered by the integration and service tests. Browser-only invalid form feedback and responsive/accessibility checks remain manual.

### T021 Review Findings for Later Tasks

- T022: The current `site.css` contains shared focus styling but no homepage-specific responsive or plant-themed rules. Verify the live layouts and contrast before adding narrowly scoped styles.
- T023: No homepage-specific JavaScript is currently needed. Verify the Bootstrap menu behavior in the browser without adding custom interaction logic.
- T024: Static accessibility checks pass for headings, sections, labels, validation help association, meaningful navigation labels, and native submit behavior. Manual focus and validation interaction remain pending.
- T025: The plant emoji is marked `aria-hidden="true"` and the adjacent PlantWise text remains visible. Verify this treatment with assistive technology during the manual review.

## Review Gate

Before implementation is accepted, the teammate responsible for the existing Index workflow must review any Index-page integration diff. Acceptance requires both `/` and `/Index` to work without route conflicts and requires the existing POST validation and Results behavior to remain unchanged.
