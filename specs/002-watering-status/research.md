# Research: Plain-Language Watering Status

## Decisions

### Use the existing .NET 10 Razor Pages application structure

- **Decision**: Implement the feature in the existing `Touching_Grass` web project, using `Models`, `Services`, `Pages`, and `Program.cs`; add tests to `tests/Touching_Grass.UnitTests`.
- **Rationale**: These are the established project locations, and the existing unit-test project already targets .NET 10 and references xUnit.
- **Alternatives considered**: A new project or test framework was unnecessary and would add maintenance or dependency cost.

### Keep status generation in a stateless service

- **Decision**: Define `IWaterStatusMessageService.Create(decimal netNeedInches)` returning a `WaterStatusMessage` with a message string. Implement the classification and formatting in `WaterStatusMessageService` and register it as a singleton.
- **Rationale**: The feature is pure deterministic logic with no state or I/O, so a singleton is safe and matches the existing service registrations. A separate service keeps the page presentation layer free of domain classification rules.
- **Alternatives considered**: Formatting in `ResultsModel` or the Razor view would violate the requested separation; a scoped service or separate project provides no benefit for stateless logic.

### Round before classifying and format amounts invariantly

- **Decision**: Round the nonnegative `decimal` using two digits and `MidpointRounding.AwayFromZero`, then compare the rounded value against zero and 0.25. Format a nonzero amount with exactly two decimal places and invariant culture.
- **Rationale**: The same rounded number determines the status category and the displayed amount; invariant formatting guarantees the required period decimal separator and stable fixed English copy.
- **Alternatives considered**: Classifying the unrounded amount could produce a message that contradicts its displayed amount. Locale-sensitive formatting could violate the specified exact amount shape.

### Reject negative needs and do not show them as recommendations

- **Decision**: `WaterStatusMessageService` throws `ArgumentOutOfRangeException` for a negative input. `ResultsModel` handles that invalid recommendation explicitly and renders no recommendation status, using the existing unavailable/invalid recommendation page feedback pattern.
- **Rationale**: Rejection is explicit, prevents invalid values from appearing as valid guidance, and leaves the existing calculation untouched. The calculator currently clamps its generated net need to zero, so negative values are invalid input to this new service boundary rather than a normal calculation result.
- **Alternatives considered**: Clamping a negative value to zero would hide invalid data and contradict the specification; allowing a formatted negative amount would incorrectly present it as a valid recommendation.

### Preserve existing calculation and dependencies

- **Decision**: Do not modify `WateringCalculatorService` or its inputs/formula, and add no packages. Do not create an external contract artifact because the feature exposes no external API or system integration.
- **Rationale**: These choices directly satisfy the feature constraints and the constitution's maintainability and testable-delivery principles.
- **Alternatives considered**: Altering or duplicating the calculation, adding a new dependency, or documenting an external API would expand scope without user value.
