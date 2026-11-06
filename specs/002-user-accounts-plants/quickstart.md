# Quickstart: Account and Personal Plants Feature

## Purpose

This guide defines user-level and artifact-level verification after implementation. It does not prescribe source-code structure beyond the approved feature boundaries.

## Preconditions

- The existing .NET 10 Razor Pages application starts successfully.
- ASP.NET Core Identity and the Identity Entity Framework store are configured.
- SQL Server LocalDB is running on the development machine.
- The initial EF migration has been applied to the feature test database.
- `plants.json` and `zip-locations.json` are available as read-only reference datasets.
- The current `WateringCalculatorService` and deterministic mock-weather values are available.
- No external weather API is configured or called by this feature.
- Two distinct test email addresses, two supported ZIP codes, and two source-dataset plants are available.

## Verification flow

1. Register User A with a supported ZIP and an email/password.
2. Confirm the account and default ZIP persist in the relational database; confirm no account data is written to JSON.
3. Sign in with User A and access protected profile and personal-plant pages.
4. Attempt invalid credentials and confirm a generic response that does not reveal account existence or echo the password.
5. Complete a successful existing plant search as User A. Confirm the recommendation succeeds before one personal-plant record is created.
6. Repeat the same search. Confirm no duplicate is created, the favorite state is preserved, and only the approved search timestamp changes.
7. Force or simulate calculation failure. Confirm no personal-plant record is created or modified.
8. Save a second plant, use the `OnPostToggleFavoriteAsync` personal-list handler, and confirm the server-rendered control has visible/accessibility text and favorites-first ordering.
9. Sign out, sign back in, and confirm personal plants and favorite state persist.
10. Open the saved plant and confirm current common name, scientific name when available, current default ZIP, calculated need and units, existing explanation, and deterministic mock-weather context where exposed.
11. Compare the saved-plant result with the anonymous search result using the same plant, ZIP, water factor, and mock-weather values; confirm parity.
12. Update the default ZIP to another supported ZIP, reopen the saved plant, and confirm recalculation uses the new ZIP.
13. Attempt malformed and unknown ZIP updates; confirm clear feedback and preservation of the prior valid ZIP.
14. Make a source plant unavailable and confirm its saved record remains visible using snapshots, is marked unavailable, preserves favorite state, and cannot open a recommendation.
15. Register User B and verify User B cannot view or modify User A's account, personal plants, favorites, or saved recommendation by changing request identifiers.
16. Sign out and attempt direct access to protected pages/actions; confirm safe redirection or denial.
17. Exercise the original anonymous plant-and-ZIP flow and confirm its prior behavior remains functional.

## Security and logging verification

- Review all new logging, exception handling, and serialization code.
- Confirm passwords, password hashes, security stamps, authentication cookies, connection strings, secrets, antiforgery tokens, full account records, stack traces, database details, and unnecessary profile data are absent from logs and user-facing errors.
- Confirm detailed exception information is limited to appropriate development diagnostics.
- Document the review result in the feature verification evidence.

## Performance verification

Run in local development or automated integration-test execution with SQL Server LocalDB on the same machine, Release configuration, deterministic mock weather, one active test user, and only feature-test data seeded. Exclude startup, first migration, and debugger overhead. Warm up once, execute registration and sign-in at least 10 times each, measure at the application request boundary, and record environment, build configuration, iteration count, median, slowest result, and pass/fail against SC-001 and SC-002.

## Completion evidence

Record results for SC-001 through SC-008, including Identity/LocalDB setup, personal-data isolation, upsert timing, duplicate prevention, favorite accessibility, unavailable-source handling, mock-weather recommendation parity, anonymous regression, security/logging review, and performance measurements.
