# Account and Personal Plants Contract

This contract describes user-visible Razor Pages behavior for the feature. Account persistence uses ASP.NET Core Identity with the Identity Entity Framework store; mutable personal data uses Entity Framework Core and SQL Server LocalDB for local development. Existing Plant and ZIP JSON files remain read-only reference datasets.

## Anonymous account actions

### Register

- **Input**: Required email address, password, exact-match `ConfirmPassword`, and default five-digit ZIP code. Leading and trailing email whitespace is removed before server-side ASP.NET Core Identity email validation and configured normalized-email comparison.
- **Success**: Account is created with the validated default ZIP and the user receives a clear sign-in path.
- **Validation failure**: Field-level messages identify missing or malformed email, password-policy failure, missing or mismatched `ConfirmPassword`, or invalid ZIP input; no password value is echoed or redisplayed, and no partial account, default ZIP, or profile data is retained. Password and ConfirmPassword are not logged or retained in form fields after failed submission.
- **Duplicate email**: Generic already-registered message; do not reveal account details.

### Sign in

- **Input**: Email address and password.
- **Success**: Authenticated session and access to the signed-in user's personal pages.
- **Failure**: Generic authentication failure; do not reveal whether the email exists, whether the password was wrong, or other account state.
- **Password policy**: Configure ASP.NET Core Identity explicitly with `RequiredLength = 8`, `RequiredUniqueChars = 1`, `RequireDigit = true`, `RequireLowercase = true`, `RequireUppercase = true`, and `RequireNonAlphanumeric = false`. Delegate hashing, verification, policy enforcement, account lockout behavior when enabled, and safe failures to Identity; do not implement custom validation, hashing, or readable password storage.

### Sign out

- **Success**: Authenticated session ends; protected personal pages and actions require sign-in again.

## Authenticated account actions

### View/update profile

- **Read**: Return only the signed-in user's account settings and current default ZIP.
- **Update**: Accept only a five-digit ZIP present in the ZIP-location dataset; preserve the prior valid ZIP when validation fails.

### List personal plants

- **Read**: Return only the signed-in user's personal entries, sorted favorites first with stable ordering within each group.
- **Empty state**: Return a clear message when no entries exist.
- **Unavailable source plant**: Return an entry-level unavailable state without failing the whole list.

### Favorite/unfavorite

- **Input**: Personal plant record selected by the user.
- **Authorization**: The selected record must belong to the authenticated user.
- **Success**: Update only that user's favorite status and return the updated visible status/order.
- **Failure**: Safe failure without revealing whether another user's record exists.

### Open saved plant recommendation

- **Input**: A personal plant record belonging to the authenticated user.
- **Processing**: Resolve the referenced source plant, resolve the user's current default ZIP, use the existing deterministic mock-weather inputs, and call the existing `WateringCalculatorService`.
- **Success**: Display selected plant, ZIP used, calculated water need, and the existing explanation.
- **Unavailable data**: If the persisted default ZIP no longer resolves, do not calculate the recommendation, invoke `WateringCalculatorService` with invented or replacement coordinates, or select a fallback ZIP. Preserve the persisted default ZIP, personal plant record, favorite status, `CreatedAt`, and `LastSearchedAt`; display a safe actionable message explaining that the saved ZIP is unavailable and direct the user to update it from the profile page. Keep navigation to personal pages available and do not expose personal information, database details, or internal exception information. This state is distinct from malformed user-entered ZIP input, an unknown registration ZIP, missing plant data, and unavailable or invalid mock-weather inputs.
- **Result content**: Display current common name, scientific name when available, ZIP used, calculated water need and units, and the existing explanation. Do not display internal database identifiers.

## Personal-plant upsert contract

- **Timing**: After ZIP resolution, plant resolution, and successful recommendation calculation, and before the successful recommendation response or redirect.
- **First successful search**: After a successful recommendation, create the `(authenticated user, SourcePlantId)` record with `CreatedAt` and `LastSearchedAt` set to the current UTC timestamp. `CreatedAt` is set once, is not client-supplied, and is not changed by later operations.
- **Repeat successful search**: Update the existing record's `LastSearchedAt` with the current UTC timestamp, preserve `CreatedAt` and favorite state, and create no duplicate.
- **Timestamp exclusions**: Do not update either timestamp after list viewing, plant-details viewing, favorite toggling, unavailable-source rendering, failed validation, ZIP or plant lookup, unavailable or invalid mock-weather inputs, or failed recommendation calculation. Favorite changes preserve both timestamps.
- **Failure**: Invalid input, lookup failure, or calculation failure creates or changes no personal-plant record.
- **Atomicity**: The relational upsert commits atomically; JSON lookups and deterministic mock-weather calculation remain read-only operations outside a distributed transaction.

## Favorite action contract

- **Location**: Dedicated POST handler on the protected personal-plants PageModel at `Pages/MyPlants/Index.cshtml.cs`.
- **Handler**: Use one `OnPostToggleFavoriteAsync` handler.
- **Protection**: Require authentication and antiforgery protection; query the selected personal record within the signed-in user's ownership scope.
- **Success**: Persist the toggled state and return to the personal-plant list with favorites first.
- **Control**: Use a server-rendered, keyboard-operable button with visible or accessible text such as `Add [plant name] to favorites` or `Remove [plant name] from favorites`; do not rely only on color or icon shape.

## Authorization invariants

- No personal operation accepts caller-supplied ownership as authoritative.
- Signed-out requests cannot read or mutate account, personal plant, or favorite data.
- Cross-user requests return the same safe denial/not-found style regardless of whether a target record exists.
- Errors do not include passwords, credentials, secrets, or unrelated users' data.
- Logs and responses must exclude passwords, password hashes, security stamps, authentication cookies, connection strings, secrets, antiforgery tokens, full account records, stack traces, and database details. Expected authentication failures must not log submitted credentials.
