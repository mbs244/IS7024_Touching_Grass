# Feature Specification: Plain-Language Watering Status

**Feature Branch**: `WaterStatusMessage_guylergm`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "Show a plain-language watering status on the Results page based on the calculated net watering need. If the need rounds to 0.00 in, show ‘No extra watering needed this week.’ If it is above 0.00 and below 0.25 in, show ‘Water lightly this week: about X.XX in.’ If it is 0.25 in or more, show ‘Water about X.XX in this week.’ Classify using the need rounded to two decimals (away from zero) so the message never contradicts the displayed number. Amounts use two decimals and the unit ‘in’. A negative need is invalid and must be rejected. The existing watering calculation remains unchanged; status rules stay outside page presentation; render the message in a role=\"status\" region. Localization, per-plant thresholds, and weather-based wording are out of scope."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Understand the weekly watering recommendation (Priority: P1)

As a gardener reviewing my results, I want a plain-language status that reflects the calculated net watering need so I can quickly decide whether and how much to water this week.

**Why this priority**: This is the primary purpose of the feature and makes the calculated recommendation easy to act on.

**Independent Test**: Provide nonnegative calculated needs representing zero, light watering, and standard watering, then verify the exact status and displayed amount for each.

**Acceptance Scenarios**:

1. **Given** a need that rounds to 0.00 in, **When** I view the results, **Then** I see “No extra watering needed this week.” and no amount is presented as needed.
2. **Given** a need that rounds above 0.00 in and below 0.25 in, **When** I view the results, **Then** I see “Water lightly this week: about X.XX in.” with the rounded need substituted for X.XX.
3. **Given** a need that rounds to 0.25 in or more, **When** I view the results, **Then** I see “Water about X.XX in this week.” with the rounded need substituted for X.XX.

---

### User Story 2 - Trust the displayed amount and wording (Priority: P2)

As a gardener, I want the status category and the displayed amount to use the same rounded value so the wording does not contradict the number I see.

**Why this priority**: A mismatch at rounding or category boundaries could undermine trust and lead to an incorrect watering decision.

**Independent Test**: Check values around rounding and the 0.25 in boundary, including midpoint values, and confirm the displayed amount and category agree; also verify that a negative need is rejected.

**Acceptance Scenarios**:

1. **Given** a nonnegative need of 0.004 in, **When** I view the results, **Then** it is rounded to 0.00 in and classified as no extra watering.
2. **Given** a nonnegative need of 0.005 in, **When** I view the results, **Then** it is rounded away from zero to 0.01 in and classified as light watering.
3. **Given** a need of 0.244 in, **When** I view the results, **Then** it is shown as 0.24 in and classified as light watering.
4. **Given** a need of 0.245 in, **When** I view the results, **Then** it is shown as 0.25 in and classified as standard watering.
5. **Given** any displayed nonzero need, **When** I read the status, **Then** its amount has exactly two decimal places and uses the unit “in”.
6. **Given** a negative calculated need, **When** the Results page attempts to present the recommendation, **Then** the need is rejected and no valid watering status is shown.

---

### User Story 3 - Receive an accessible status update (Priority: P3)

As a gardener using assistive technology, I want the watering status announced as a status update so I can receive the recommendation without relying on visual presentation alone.

**Why this priority**: The recommendation must be available to people using assistive technology as well as sighted users.

**Independent Test**: Inspect the rendered results and verify that the message is exposed through a region with status semantics that assistive technology can announce.

**Acceptance Scenarios**:

1. **Given** the Results page presents a watering status, **When** assistive technology encounters it, **Then** the message is exposed in a `role="status"` region.

---

### Edge Cases

- A negative calculated need is invalid and must be rejected; it must not be presented as a valid watering recommendation.
- Rounding is applied before selecting a status category, including at the 0.00 in and 0.25 in boundaries.
- Midpoint values are rounded away from zero to two decimal places.
- The status must not suggest a category inconsistent with the displayed rounded amount.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Results page MUST present a plain-language weekly watering status based on the calculated net watering need.
- **FR-002**: The system MUST round a nonnegative need to two decimal places, using midpoint rounding away from zero, before selecting a status category.
- **FR-003**: When the rounded need is 0.00 in, the status MUST read exactly: “No extra watering needed this week.”
- **FR-004**: When the rounded need is greater than 0.00 in and less than 0.25 in, the status MUST read “Water lightly this week: about X.XX in.”, with X.XX replaced by the rounded need.
- **FR-005**: When the rounded need is 0.25 in or greater, the status MUST read “Water about X.XX in this week.”, with X.XX replaced by the rounded need.
- **FR-006**: Any nonzero amount shown in the status MUST use exactly two decimal places and the unit “in”; its value MUST be the same rounded value used to classify the status.
- **FR-007**: The system MUST reject a negative calculated need and MUST NOT present it as a valid watering status.
- **FR-008**: The status classification rules MUST remain separate from the page’s presentation responsibility, and the existing watering calculation MUST remain unchanged.
- **FR-009**: The status message MUST be rendered in a `role="status"` region so assistive technology can identify it as a status update.
- **FR-010**: The feature MUST NOT add localization, per-plant thresholds, or weather-based wording.

### Key Entities

- **Calculated net watering need**: The nonnegative amount of additional water recommended for the current week, expressed in inches and provided by the existing calculation.
- **Watering status**: The plain-language weekly message and, when nonzero, its two-decimal-inch amount, selected from the rounded need.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In all acceptance cases spanning 0.00 in, values between 0.00 and 0.25 in, exactly 0.25 in, and values above 0.25 in, the displayed message matches the specified wording for the rounded need.
- **SC-002**: In all tested midpoint cases, the need is rounded to two decimal places away from zero before the message category is selected.
- **SC-003**: Every nonzero amount in a displayed status has exactly two decimal places, is followed by “in”, and matches the value used to select its wording.
- **SC-004**: No negative need is shown as a valid watering status.
- **SC-005**: Every displayed watering status is available in a `role="status"` region and can be identified as a status update by assistive technology.

## Assumptions

- The feature applies to the single weekly recommendation shown on the Results page; it does not calculate or modify the recommended need.
- The supplied status phrases are fixed English copy for this feature; localization is out of scope.
- The amount substituted for X.XX is the rounded need, not the unrounded calculation result.
- A negative need is an invalid input to the status feature and is rejected rather than clamped to zero.
- No per-plant classification thresholds or weather-driven wording are introduced.
