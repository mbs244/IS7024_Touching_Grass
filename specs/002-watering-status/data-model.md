# Data Model: Plain-Language Watering Status

## WaterRecommendation (existing, unchanged)

- **Purpose**: Carries the existing weekly recommendation from the calculation to the Results page.
- **Relevant field**: `NetNeedInches` (`decimal`), the calculated net watering need in inches.
- **Relationship**: The status service receives this value; the new feature does not alter its calculation or persistence/serialization shape.

## WaterStatusMessage (new)

- **Purpose**: Represents the plain-language status displayed for a weekly net watering need.
- **Fields**:
  - `Message` (`string`): Exact user-facing status copy. It is the no-extra-watering phrase or one of the two amount-bearing phrases specified in the feature requirements.
- **Lifecycle**: Created from a `NetNeedInches` value for a Results request; not persisted.

## Status classification and validation

1. The input is a decimal amount in inches.
2. A value less than zero is invalid and is rejected with `ArgumentOutOfRangeException`; it is never converted into a valid status.
3. Round nonnegative values to two decimal places using midpoint rounding away from zero.
4. If the rounded value is zero, the message is `No extra watering needed this week.` and contains no amount.
5. If the rounded value is greater than zero and less than 0.25, the message is `Water lightly this week: about X.XX in.`
6. If the rounded value is at least 0.25, the message is `Water about X.XX in this week.`
7. In amount-bearing messages, `X.XX` is the rounded value formatted with exactly two decimal places and invariant culture. The category and displayed amount therefore use the same value.

## Relationships and ownership

`ResultsModel` passes `WaterRecommendation.NetNeedInches` to `IWaterStatusMessageService` and exposes the returned `WaterStatusMessage` for rendering. The Razor view renders its message as text inside a `role="status"` region; it does not classify, round, or calculate the need.
