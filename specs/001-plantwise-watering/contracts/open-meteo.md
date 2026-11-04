# Open-Meteo Integration Contract

## Client

`OpenMeteoService` is a typed HTTP client. The base URL, timeout, and retry policy are
configuration values; credentials are not required by the feature contract.

## Request

The service sends the resolved latitude and longitude, the local timezone, a seven-day
calendar range, and the provider's daily evapotranspiration variable. The exact provider
parameter names and supported variable must be verified against the Open-Meteo API
contract during implementation and covered by HTTP contract tests.

## Normalized response

```text
WeatherObservation
- Metric: Evapotranspiration
- TotalInches: non-negative decimal
- PeriodStart: local DateOnly
- PeriodEnd: local DateOnly, seven calendar days
- TimeZoneId: resolved ZIP location timezone
- Source: Open-Meteo
```

The adapter sums seven daily values and converts the documented provider unit to inches.
Missing, negative, non-numeric, or incompatible values return an unavailable-data result.

## Failure behavior

- Timeout, cancellation, network failure, non-success status, rate limit, or malformed
  JSON MUST become a typed provider failure for the application layer.
- The service MUST not expose raw response bodies or credentials to the Razor Page.
- Structured logs MAY include status code, request correlation identifier, and elapsed
  time, but MUST NOT include secrets.
