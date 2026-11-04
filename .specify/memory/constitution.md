<!--
Sync Impact Report
- Version change: unratified scaffold → 1.0.0
- Modified principles: placeholder principles → Code Quality, Testable Delivery,
  Consistent User Experience, Performance by Design, and Maintainable Simplicity
- Added sections: Engineering Constraints; Development Workflow
- Removed sections: none
- Follow-up TODOs: RATIFICATION_DATE remains TODO because the original adoption date
  was not available in the repository.
-->

# PlantWise Constitution

## Core Principles

### I. Code Quality and Clarity
Production code MUST be readable, focused, and consistent with the existing .NET 10
Razor Pages architecture. Changes MUST use clear names, appropriate null and input
validation, asynchronous I/O where applicable, and existing shared abstractions before
introducing duplicates. Compiler warnings and analyzer findings introduced by a change
MUST be resolved or explicitly justified in review. This keeps domain calculations and
external-data integrations understandable and safe to change.

### II. Testable Delivery
Every behavior change MUST include automated tests at the appropriate level. Validation,
plant-selection rules, weather-data handling, and watering calculations MUST have unit
tests covering normal, boundary, invalid, and unavailable-data cases. Razor Page workflows
and external service or persistence contracts MUST have integration tests when their
behavior crosses a process, database, or HTTP boundary. A change MUST NOT be considered
complete while its relevant tests fail or its acceptance behavior is untested. This
protects water-use recommendations from regressions.

### III. Consistent User Experience
User-facing flows MUST provide consistent page structure, terminology, validation,
feedback, and accessible form behavior across Razor Pages. Labels, error messages, units,
loading or unavailable-data states, and success confirmations MUST be understandable to
the intended gardener and must not expose implementation details. New interactions MUST
preserve keyboard accessibility, meaningful validation summaries, and responsive layouts.
Consistency reduces user error when entering ZIP codes, selecting plants, and interpreting
watering recommendations.

### IV. Performance by Design
User-facing requests MUST avoid unnecessary network, database, and repeated computation
work. External HTTP calls MUST use configured timeouts, cancellation where supported,
and safe reuse of clients; database queries MUST retrieve only required data and use
appropriate asynchronous APIs. Pages MUST remain responsive under expected classroom and
project usage, and changes that materially affect response time, query count, or payload
size MUST include evidence from tests or local measurement. Performance safeguards keep
weather lookups and recommendations usable on ordinary connections.

### V. Maintainable Simplicity
Features MUST solve the stated PlantWise user need with the smallest understandable
design. Domain rules for USA locations, plant water needs, rainfall,
evapotranspiration, and calculations MUST remain explicit and separated from page
presentation and external-service plumbing. New dependencies, abstractions, and cached
data MUST have a documented benefit and a clear ownership or invalidation strategy.
Prefer incremental designs that can be observed, tested, and removed without broad
rewrites.

## Engineering Constraints

The application targets .NET 10 and uses Razor Pages as its primary web presentation
model. User input MUST be validated at the boundary and treated as untrusted. Weather,
plant, and location data MUST identify their source and handle unavailable or stale data
without presenting an estimate as certain. Calculated water amounts MUST retain explicit
units and precision rules so that displayed values can be reconciled with source data.

## Development Workflow

Each change MUST describe its user or maintenance impact, include relevant automated
tests, and pass the project build before merge. Reviewers MUST check compliance with this
constitution, input and error handling, accessibility and UX consistency, test coverage,
and performance impact. API, schema, calculation, or user-facing contract changes MUST
include updated documentation or acceptance criteria. Known limitations MUST be recorded
instead of being hidden by disabled tests or ignored failures.

## Governance
This constitution governs project quality decisions and takes precedence over conflicting
local conventions unless an amendment is approved. Amendments MUST state the motivation,
affected principles, compatibility impact, and migration or rollout plan when applicable.
The version uses semantic versioning: MAJOR for incompatible principle changes or
removals, MINOR for new or materially expanded principles, and PATCH for clarifications
that do not change governance. Compliance MUST be reviewed during pull-request review
and at feature completion; exceptions MUST be time-bound, documented, and approved by
the project team.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date not available | **Last Amended**: 2026-11-03
