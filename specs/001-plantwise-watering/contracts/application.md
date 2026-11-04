# Application Contracts

## Razor Page form

`Pages/Index.cshtml` posts the ZIP code and plant name to `IndexModel.OnPostAsync`.
The PageModel validates and normalizes input, orchestrates services, and maps typed
outcomes to the shared Bootstrap UI states:

- Validation error: field-level messages and validation summary.
- Unsupported input: clear location or plant message; no provider call.
- Loading: submit action indicates work is in progress and prevents duplicate submission.
- Recommendation: weekly inches, formula inputs, source labels, period, and explanation.
- Temporary provider failure: generic message and retry path; no partial recommendation.

## Service contracts

```text
ZipLookupService.FindAsync(zipCode, cancellationToken)
  -> ZipLocation? or UnsupportedLocation

IPlantRepository.FindAsync(plantName, cancellationToken)
  -> Unique PlantMatch, Ambiguous PlantMatch, or Missing PlantMatch

OpenMeteoService.GetWeeklyEvapotranspirationAsync(location, cancellationToken)
  -> WeatherObservation or ProviderFailure

VisualCrossingService.GetWeeklyRainfallAsync(location, cancellationToken)
  -> WeatherObservation or ProviderFailure

WateringCalculatorService.Calculate(location, plant, et, rainfall)
  -> WateringRecommendation
```

All external and database operations are asynchronous and cancellation-aware. Services
return typed results or domain failures rather than using null to represent every failure.

## Configuration contract

```text
ConnectionStrings:PlantWise
Weather:OpenMeteo:BaseUrl
Weather:OpenMeteo:TimeoutSeconds
Weather:VisualCrossing:BaseUrl
Weather:VisualCrossing:ApiKey
Weather:VisualCrossing:TimeoutSeconds
```

Production secrets MUST be provided by secure configuration. Local development uses
User Secrets or environment variables; committed configuration contains no API key.
