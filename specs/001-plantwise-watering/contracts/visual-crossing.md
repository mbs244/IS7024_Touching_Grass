# Visual Crossing Integration Contract

## Client

`VisualCrossingService` is a typed HTTP client. The API key is supplied through secure
configuration or secret storage, never source control or form input. The base URL,
timeout, and retry policy are configuration values.

## Request

The service sends the resolved latitude and longitude, the same seven complete local
calendar days used by Open-Meteo, and a request for daily precipitation totals. The exact
provider query parameter names and response fields must be verified against the Visual
Crossing API contract during implementation and covered by HTTP contract tests.

## Normalized response

```text
WeatherObservation
- Metric: Rainfall
- TotalInches: non-negative decimal
- PeriodStart: local DateOnly
- PeriodEnd: local DateOnly, seven calendar days
- TimeZoneId: resolved ZIP location timezone
- Source: Visual Crossing
```

The adapter sums seven daily rainfall values and converts the documented provider unit to
inches. Missing, negative, non-numeric, or incompatible values return an unavailable-data
result.

## Failure behavior

- Missing or invalid API key, timeout, cancellation, network failure, non-success status,
  rate limit, or malformed JSON MUST become a typed provider failure.
- Configuration failures MUST be logged without logging the key.
- The UI receives only the generic temporary data-unavailability message and retry action.
- The service MUST not expose raw response bodies or provider credentials.
