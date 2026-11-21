# Implementation Plan: Plain-Language Watering Status

**Branch**: `WaterStatusMessage_guylergm` | **Date**: 2026-11-15 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-watering-status/spec.md`

## Summary

Add a plain-language weekly watering status to the Results page, based on `WaterRecommendation.NetNeedInches`. A stateless `IWaterStatusMessageService` will validate the nonnegative decimal input, round it to two decimal places using midpoint rounding away from zero, select the exact specified message, and return a `WaterStatusMessage`. Register the implementation as a singleton, inject it into `ResultsModel`, and render the message in an accessible `role="status"` region. Leave `WateringCalculatorService` and its calculations unchanged.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: ASP.NET Core Razor Pages; no new dependencies

**Storage**: None; status is computed per Results request and is not persisted

**Testing**: Existing xUnit project `tests/Touching_Grass.UnitTests`; existing .NET test SDK

**Target Platform**: ASP.NET Core web application supported by .NET 10

**Project Type**: Razor Pages web application

**Performance Goals**: In-process decimal formatting and classification; no additional I/O or external calls

**Constraints**: Preserve current watering calculation; fixed English message and `in` units; midpoint rounding away from zero; reject negative input; keep status rules in service; expose the rendered message with status semantics

**Scale/Scope**: One status for the weekly recommendation on the Results page; no persistence or per-plant thresholds

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Code Quality and Clarity**: Pass. Use focused model/service types, explicit validation, and existing project conventions.
- **Testable Delivery**: Pass. Add xUnit coverage for exact message text, rounding/category boundaries, ResultsModel handoff, and negative input rejection; validate with the test project and application build.
- **Consistent User Experience**: Pass. Preserve specified copy and units; render the status in an accessible `role="status"` region.
- **Performance by Design**: Pass. Pure in-process formatting and classification add no I/O or repeated weather/calculation work.
- **Maintainable Simplicity**: Pass. Add only the requested model, interface, implementation, page integration, and tests; no new dependencies.
- **Engineering Constraints**: Pass. Keep the existing calculation unchanged and retain explicit decimal precision/units.

## Project Structure

### Documentation (this feature)

```text
specs/002-watering-status/
├── plan.md              # This implementation plan
├── research.md          # Decisions and rationale
├── data-model.md        # Status message model and validation
├── quickstart.md        # Build, test, and manual verification steps
└── tasks.md             # Phase 2 output (/speckit-tasks; not created here)
```

No `contracts/` directory is needed: this feature adds no external API or system integration; its service is an internal application abstraction.

### Source Code (repository root)

```text
Touching_Grass/
├── Models/
│   ├── WaterRecommendation.cs             # Existing; unchanged
│   └── WaterStatusMessage.cs              # New status text model
├── Services/
│   ├── IWaterStatusMessageService.cs       # New status service contract
│   ├── WaterStatusMessageService.cs        # New formatting/classification logic
│   └── WateringCalculatorService.cs        # Existing; unchanged
├── Pages/
│   ├── Results.cshtml.cs                   # Inject and invoke status service
│   └── Results.cshtml                      # Render role="status" region
└── Program.cs                              # Register status service singleton

tests/Touching_Grass.UnitTests/
├── WaterStatusMessageServiceTests.cs       # New xUnit boundary and invalid-input tests
└── ResultsModelTests.cs                    # New status handoff and rejection tests
```

**Structure Decision**: Follow the existing single ASP.NET Core web project, with domain models in `Touching_Grass/Models`, services in `Touching_Grass/Services`, Results presentation in `Touching_Grass/Pages`, DI registration in `Program.cs`, and unit tests in the existing xUnit project.

## Complexity Tracking

No constitution violations or additional projects/dependencies are introduced.
