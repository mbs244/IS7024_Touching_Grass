# Feature Specification: PlantWise Watering Recommendation

**Feature Branch**: `001-plantwise-watering`
**Created**: 2026-11-03
**Status**: Draft
**Input**: Create a PlantWise feature specification for ZIP and plant-based watering recommendations using local datasets and weather APIs.

## Clarifications

### Session 2026-11-03

- Q: What measurement period and unit should PlantWise use for evapotranspiration,
  rainfall, and the displayed watering recommendation? → A: Weekly totals in inches.
- Q: How should the local plant water factor be represented and applied in the
  calculation? → A: Decimal from 0 to 1.
- Q: What should PlantWise do when the Visual Crossing API credential is missing or
  invalid? → A: Fail safely, log the configuration error, and show a generic retry
  message.
- Q: How should PlantWise produce weekly evapotranspiration and rainfall totals from
  the weather APIs? → A: Sum seven complete calendar days using each provider's daily
  values.
- Q: Which timezone should define the seven complete calendar days used for the weekly
  weather totals? → A: Use the resolved location's local timezone.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Submit a watering lookup (Priority: P1)

As a California gardener, I want to enter my ZIP code and plant name so that PlantWise
can provide a location-specific watering recommendation.

**Why this priority**: This is the primary user value and the smallest complete journey.

**Independent Test**: Enter a supported ZIP code and known plant, submit the form, and
verify that a recommendation with an explanation is displayed.

**Acceptance Scenarios**:

1. **Given** a supported five-digit ZIP code and a plant present in the local plant
   dataset, **When** the user submits the lookup, **Then** the system resolves the ZIP
   code, retrieves the required weather data, calculates water need, and displays a
   recommendation and explanation.
2. **Given** a valid ZIP code and plant name, **When** the user submits the form,
   **Then** the page shows the submitted values or their resolved names alongside the
   recommendation so the result is easy to interpret.
3. **Given** the calculated water need is zero or negative, **When** the result is
   displayed, **Then** the recommendation states that no additional watering is needed
   for the measured period and does not display a negative amount.

### User Story 2 - Understand the calculation (Priority: P1)

As a gardener, I want to see how my recommendation was determined so that I can trust
and act on it.

**Why this priority**: A transparent result is necessary for users to distinguish a
weather-based estimate from a guaranteed instruction.

**Independent Test**: Provide known evapotranspiration, plant factor, and rainfall
values, then verify the displayed result and explanation use the required formula and
units.

**Acceptance Scenarios**:

1. **Given** evapotranspiration, plant water factor, and rainfall values, **When** the
   calculation completes, **Then** water need equals `(Evapotranspiration × Plant Water
   Factor) - Rainfall`.
2. **Given** a completed calculation, **When** the recommendation is displayed, **Then**
   the explanation identifies the evapotranspiration amount, plant water factor,
   rainfall total, calculated result, and measurement units.
3. **Given** a weather value or plant factor is unavailable, **When** the system cannot
   calculate a complete result, **Then** it displays a clear unavailable-data message
   instead of inventing or silently substituting a value.

### User Story 3 - Recover from invalid input or service failure (Priority: P1)

As a gardener, I want actionable feedback when my input is invalid or a data provider is
unavailable so that I know how to proceed.

**Why this priority**: Validation and failure handling protect users from misleading
recommendations and provide a usable experience when dependencies fail.

**Independent Test**: Submit invalid values and simulate each data-source failure, then
verify that no recommendation is shown and the page provides an understandable recovery
message.

**Acceptance Scenarios**:

1. **Given** an empty or malformed ZIP code, **When** the user submits the form,
   **Then** the system shows the ZIP validation message and does not call either weather
   API.
2. **Given** a ZIP code that is valid in format but absent from the local ZIP dataset,
   **When** the user submits the form, **Then** the system explains that the location is
   unsupported and does not call either weather API.
3. **Given** a plant name that is empty or absent from the local plant dataset, **When**
   the user submits the form, **Then** the system shows an actionable plant validation
   message and does not calculate a recommendation.
4. **Given** an external weather API request fails, times out, is rate-limited, or
   returns unusable data, **When** the user submits a lookup, **Then** the system shows a
   temporary data-availability message, avoids exposing provider details, and offers the
   user a retry path.

### Edge Cases

- ZIP input includes leading or trailing whitespace; the system trims it before format
  validation without changing the user-visible value unexpectedly.
- ZIP input contains letters, punctuation, fewer than five digits, or more than five
  digits; the system rejects it as invalid.
- A valid ZIP code maps to incomplete or ambiguous latitude/longitude data; the lookup
  is treated as unavailable and no weather request is made.
- A plant search differs only by capitalization or surrounding whitespace; matching is
  case-insensitive and whitespace-tolerant.
- Multiple local plant records match a name; the system selects the uniquely identified
  record or asks the user to choose, and never selects arbitrarily.
- Evapotranspiration or rainfall is zero; the calculation still completes using the
  available zero value.
- Rainfall exceeds estimated plant water use; the displayed recommendation is zero,
  with an explanation that rainfall offsets the estimated need.
- A provider returns a negative, nonsensical, missing, or differently unitized weather
  value; the value is rejected unless it can be safely normalized according to a known
  contract.
- The user submits repeated requests; the page remains responsive and does not create
  duplicate or conflicting recommendations.
- A provider responds slowly; the request uses a bounded wait and the user receives a
  clear retry message when the bound is exceeded.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a Razor Pages form that accepts a ZIP code and
  plant name.
- **FR-002**: The system MUST trim user input and validate that the ZIP code contains
  exactly five digits before any external API request.
- **FR-003**: The system MUST reject an empty ZIP code with the message "ZIP code is
  required.".
- **FR-004**: The system MUST reject a malformed ZIP code with the message "Enter a
  valid five-digit ZIP code.".
- **FR-005**: The system MUST resolve a valid ZIP code to latitude and longitude using
  the local ZIP code dataset.
- **FR-006**: The system MUST report a valid-format ZIP code as unsupported when it is
  absent from the local dataset, without calling a weather API.
- **FR-007**: The system MUST require a non-empty plant name and reject a plant that is
  not present in the local plant dataset.
- **FR-008**: The system MUST look up the selected plant's water factor from the local
  plant dataset, where the factor is a decimal from 0 to 1, and apply it directly to
  weekly evapotranspiration measured in inches.
- **FR-009**: The system MUST retrieve daily evapotranspiration data from the Open-Meteo
  API using the resolved location for seven complete calendar days and sum those daily
  values into a weekly total in inches.
- **FR-010**: The system MUST retrieve daily rainfall data from the Visual Crossing API
  using the resolved location for the same seven complete calendar days and sum those
  daily values into a weekly total in inches.
- **FR-010a**: The system MUST define the seven complete calendar days using the local
  timezone of the resolved ZIP-code location and use that same period for both weather
  providers.
- **FR-011**: The system MUST validate that weather responses contain usable values and
  expected units before calculation.
- **FR-012**: The system MUST calculate water need as `(Evapotranspiration × Plant
  Water Factor) - Rainfall`.
- **FR-013**: The system MUST clamp a negative calculated water need to zero for the
  displayed recommendation while retaining enough detail to explain the rainfall
  offset.
- **FR-014**: The system MUST display the watering recommendation with an explicit unit,
  the measurement period, and an explanation of the input values used.
- **FR-015**: The system MUST display validation errors next to or in a summary near the
  relevant form fields and preserve accessible labels and keyboard operation.
- **FR-016**: The system MUST handle Open-Meteo and Visual Crossing failures, timeouts,
  rate limits, malformed responses, and unavailable values without displaying a
  fabricated recommendation.
- **FR-016a**: The system MUST treat a missing or invalid Visual Crossing credential as
  a failed weather lookup, log the configuration problem without logging the credential,
  and show the same generic temporary data-availability message used for recoverable
  provider failures.
- **FR-017**: The system MUST use bounded requests and support cancellation for external
  weather lookups where the request lifecycle permits it.
- **FR-018**: The system MUST avoid exposing API credentials, raw provider responses, or
  implementation details in user-facing error messages.
- **FR-019**: The system MUST provide a retry path after a recoverable external-service
  failure.
- **FR-020**: The system MUST log actionable provider failure details without logging
  secrets or unnecessary user data.

### Validation Rules

| Field | Rule | User-facing behavior |
|---|---|---|
| ZIP code | Required; exactly five numeric digits after trimming | Show a required or five-digit validation message; do not call weather APIs when invalid |
| ZIP code location | Must exist in the local ZIP dataset with usable coordinates | Explain that the location is unsupported or unavailable |
| Plant name | Required after trimming | Show that a plant name is required |
| Plant record | Must match a local plant record; matching is case-insensitive | Explain that the plant is not in the supported dataset |
| Weather values | Must be present, numeric, non-negative, and in expected units | Show that a recommendation cannot be calculated from unavailable data |

### API Failure Scenarios

| Dependency | Failure | Required behavior |
|---|---|---|
| Open-Meteo | Timeout, network error, rate limit, non-success response, or malformed payload | Do not calculate; show a temporary weather-data message and allow retry |
| Visual Crossing | Timeout, network error, rate limit, non-success response, or malformed payload | Do not calculate; show a temporary weather-data message and allow retry |
| Either provider | Missing, negative, stale, or incompatible-unit value | Treat the lookup as unavailable unless a documented normalization is possible |
| Both providers | One succeeds and the other fails | Do not display a partial recommendation as complete; identify that required weather data is unavailable |
| Local ZIP dataset | Missing record or unusable coordinates | Stop before API calls and explain that the location is unsupported |
| Local plant dataset | Missing or ambiguous match | Stop before calculation and request a supported or more specific plant |

### Key Entities

- **ZIP Location**: A locally maintained record connecting a five-digit ZIP code to
  latitude, longitude, local timezone, and any supported-location metadata.
- **Plant**: A locally maintained plant record containing an identifiable plant name and
  its plant water factor.
- **Weather Observation**: Provider-sourced evapotranspiration or rainfall data for a
  resolved location and measurement period, including its unit and source.
- **Watering Recommendation**: The calculated non-negative water need, input values,
  units, measurement period, and human-readable explanation shown to the user.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 95% of valid lookups with available dependencies display a
  recommendation or a clear result within 10 seconds under normal network conditions.
- **SC-002**: 100% of invalid ZIP submissions are rejected before an external weather API
  request is made.
- **SC-003**: 100% of displayed recommendations include the formula inputs, units,
  measurement period, and data-source context needed to reproduce the result.
- **SC-004**: 100% of simulated provider failures produce a user-facing recovery message
  and no fabricated or partial recommendation.
- **SC-005**: At least 90% of representative users can complete a valid lookup on their
  first attempt without assistance in usability testing.
- **SC-006**: Automated tests cover normal, boundary, invalid-input, unavailable-data,
  and external-provider failure scenarios for the lookup and calculation behavior.
- **SC-007**: Repeated valid lookups do not cause unbounded growth in outstanding
  requests, duplicated page results, or avoidable repeated local-data reads during one
  request.

## Assumptions

- The first release supports ZIP codes represented in the local dataset; unsupported
  locations are not geocoded through a third-party service.
- The local ZIP dataset provides usable latitude and longitude for every supported
	record and identifies the local timezone used for weather-day boundaries.
- The local plant dataset provides one usable plant water factor for each selectable
  plant record, represented as a decimal from 0 to 1, or the user is prompted to
  disambiguate multiple matches.
- Open-Meteo provides evapotranspiration in a documented unit that can be normalized to
  the calculation unit.
- Visual Crossing provides rainfall totals in a documented unit and measurement period
  that can be normalized to the calculation unit.
- Evapotranspiration and rainfall are weekly totals measured in inches, produced by
  summing daily values for seven complete calendar days. Both values refer to the same
  seven-day period used by the displayed recommendation.
- API credentials required by Visual Crossing are supplied through secure application
  configuration and are never entered by the user or committed to source control. A
  missing or invalid credential fails the lookup safely rather than producing a partial
  recommendation.
- A watering recommendation is an estimate for decision support, not a guarantee of
  plant health or a replacement for local horticultural judgment.
- Authentication, user accounts, saved plant lists, notifications, and historical
  recommendations are outside this feature's scope.
