# Data Model: Authenticated User Accounts and Personal Plants

## User Account

Represents a registered PlantWise user.

- **Identity**: Stable account identifier and unique normalized email address.
- **Credential data**: Secure password-verification data only; never readable password text.
- **Authentication**: ASP.NET Core Identity local email-and-password account; this feature creates no Identity roles.
- **DefaultZipCode**: Exactly five digits and must resolve in the current ZIP-location dataset.
- **Relationships**: Owns zero or more Personal Plant records.
- **Lifecycle**: Registered → active authenticated account; sign-in creates an authenticated session; sign-out ends the session.
- **Validation**: Required email and password; duplicate email registration is rejected generically; default ZIP must be present, five digits, and known.

## Plant

Represents an existing record in the PlantWise source plant dataset.

- **Identity**: Existing source Plant identifier.
- **Attributes used by this feature**: Common name, scientific name, and water factor.
- **Ownership**: Not owned by a user; shared reference data.
- **Lifecycle**: May become unavailable in the source dataset after a user has saved it; personal pages must handle that state safely.

## ZIP Location

Represents an existing record in the PlantWise ZIP-location dataset.

- **Identity**: Five-digit ZIP code.
- **Attributes used by this feature**: Latitude, longitude, and optional timezone/source location metadata.
- **Ownership**: Shared reference data.
- **Validation**: A default ZIP is valid only when it exists in the current dataset and has usable location data for recommendation lookup.

## Personal Plant

Represents a user's saved association to an existing Plant.

- **Identity**: Stable personal-entry identifier or the conceptual pair `(User Account, Plant)`, unique per user and source Plant.
- **Owner**: Exactly one User Account.
- **Plant reference**: `SourcePlantId`, `CommonNameSnapshot`, and `ScientificNameSnapshot` captured when first saved. The snapshots are display fallbacks only and never authoritative calculation inputs.
- **IsFavorite**: Boolean favorite status owned by the user.
- **CreatedAt**: Creation timestamp.
- **LastSearchedAt**: Last successful-search timestamp when retained by the approved model.
- **Ordering**: Favorites sort before non-favorites; ties use a stable deterministic order.
- **Lifecycle**: Created or upserted only after ZIP resolution, plant resolution, and successful `WateringCalculatorService` calculation. Repeated searches update the existing entry and timestamp while preserving favorite state. It remains visible if the source Plant becomes unavailable.
- **Unavailable source state**: If `SourcePlantId` no longer resolves, render the snapshots, mark the entry unavailable, preserve favorite state, disable current recommendations, and never calculate with the snapshot or stale water factor.

## Watering Recommendation

Represents the existing calculated result for a request.

- **Inputs**: Selected current Plant, user's current DefaultZipCode resolved through ZIP Location, existing deterministic mock-weather values, and the established `WateringCalculatorService` calculation.
- **Outputs**: Plant identity, ZIP used, calculated water need, units, and existing explanation.
- **Persistence**: Generated on request; not a permanent personal-plant snapshot.
- **Failure states**: Missing plant, ZIP-location, or mock-weather data produces a safe unavailable-data result rather than an invented recommendation.

## Relationships and invariants

- One User Account has exactly one current default ZIP and zero or more Personal Plant records.
- One Personal Plant belongs to exactly one User Account and references one source Plant concept.
- A user cannot have duplicate Personal Plant records for the same source Plant.
- A user's favorite state cannot affect another user's Personal Plant record.
- A recommendation uses the default ZIP value current at request time.
- All personal reads and writes are scoped to the authenticated owner.
- The personal-plant upsert is committed atomically by the relational context after calculation succeeds; it does not require a distributed transaction with read-only JSON lookup or mock-weather operations.
