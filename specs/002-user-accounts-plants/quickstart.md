# Quickstart: Account and Personal Plants Feature

## Purpose

This guide defines user-level and artifact-level verification after implementation. It does not prescribe source-code structure beyond the approved feature boundaries.

## Preconditions

- The existing .NET 10 Razor Pages application starts successfully.
- ASP.NET Core Identity and the Identity Entity Framework store are configured.
- SQL Server LocalDB is running on the development machine.
- The initial EF migration is owned by `Touching_Grass/Touching_Grass.csproj`, stored under `Touching_Grass/Data/Migrations/`, and applied to the isolated feature test database. The test project does not own or copy migrations.
- `plants.json` and `zip-locations.json` are available as read-only reference datasets.
- The current `WateringCalculatorService` and existing deterministic mock-weather inputs are available.
- No external weather API is configured or called by this feature.
- Two distinct test email addresses, two supported ZIP codes, and two source-dataset plants are available.
- Reuse the existing `tests/Touching_Grass.UnitTests/Touching_Grass.UnitTests.csproj`, which targets `net10.0`, uses xUnit with Microsoft.NET.Test.Sdk 17.14.1, xunit 2.9.3, xunit.runner.visualstudio 3.1.4, and coverlet.collector 6.0.4, references `Touching_Grass/Touching_Grass.csproj`, and is included in `Touching_Grass.slnx`.
- Record the inspected application/test `net10.0` targets, installed SDK, selected compatible application and test package names/versions, restore result, and build result. Record the explicit Identity password policy: RequiredLength = 8, RequiredUniqueChars = 1, RequireDigit = true, RequireLowercase = true, RequireUppercase = true, RequireNonAlphanumeric = false.
- Use SQL Server LocalDB for normal application development. For integration tests, create a uniquely named isolated SQL Server LocalDB database, apply the same application migration, begin from a known state, and never connect to or modify the ordinary development or production database. Do not replace these relational tests with EF Core InMemory.

## Verification flow

1. Register User A with a supported ZIP and an email/password.
2. Confirm the account and default ZIP persist in the relational database; confirm no account data is written to JSON.
3. Sign in with User A and access protected profile and personal-plant pages.
4. Attempt invalid credentials and confirm a generic response that does not reveal account existence or echo the password.
5. Complete a successful existing plant search as User A. Confirm the recommendation succeeds before one personal-plant record is created.
6. Repeat the same search. Confirm no duplicate is created, `CreatedAt` is unchanged, `LastSearchedAt` advances in UTC, and the favorite state is preserved.
7. Force or simulate calculation failure. Confirm no personal-plant record is created or modified and both timestamps remain unchanged for an existing record.
8. Confirm that viewing the personal plant list or details, toggling favorite status, displaying an unavailable source plant, or failing validation does not modify either timestamp.
9. Save a second plant, use the `OnPostToggleFavoriteAsync` personal-list handler, and confirm the server-rendered control has visible/accessibility text and favorites-first ordering.
10. Sign out, sign back in, and confirm personal plants and favorite state persist.
11. Open the saved plant and confirm current common name, scientific name when available, current default ZIP, calculated need and units, existing explanation, and deterministic mock-weather context where exposed.
12. Compare the saved-plant result with the anonymous search result using the same plant, ZIP, water factor, and existing deterministic mock-weather inputs; confirm parity.
13. Verify passwords shorter than eight characters or missing a digit, lowercase letter, or uppercase letter are rejected; verify a valid password does not require a non-alphanumeric character. Confirm validation responses never echo passwords and unsuccessful sign-in uses a generic message.
14. Update the default ZIP to another supported ZIP, reopen the saved plant, and confirm recalculation uses the new ZIP.
15. Attempt malformed and unknown ZIP updates; confirm clear feedback and preservation of the prior valid ZIP.
16. Make a source plant unavailable and confirm its saved record remains visible using snapshots, is marked unavailable, preserves favorite state, and cannot open a recommendation.
17. Register User B and verify User B cannot view or modify User A's account, personal plants, favorites, or saved recommendation by changing request identifiers.
18. Sign out and attempt direct access to protected pages/actions; confirm safe redirection or denial.
19. Exercise the original anonymous plant-and-ZIP flow and confirm its prior behavior remains functional.

20. Verify the empty application database can be created from the application migration, the isolated test database can be created from that same migration, Identity and personal-plant relationships and the user-plus-SourcePlantId uniqueness constraint are enforced, and repeated test executions do not depend on data from an earlier run.

## Security and logging verification

- Review all new logging, exception handling, and serialization code.
- Confirm passwords, password hashes, security stamps, authentication cookies, connection strings, secrets, antiforgery tokens, full account records, stack traces, database details, and unnecessary profile data are absent from logs and user-facing errors.
- Confirm detailed exception information is limited to appropriate development diagnostics.
- Document the review result in the feature verification evidence.

## Performance verification

Run in local development or automated integration-test execution with SQL Server LocalDB on the same machine, Release configuration, existing deterministic mock-weather inputs, one active test user, and only feature-test data seeded. Exclude startup, first migration, and debugger overhead. Warm up once, execute registration and sign-in at least 10 times each, measure at the application request boundary, and record environment, build configuration, iteration count, median, slowest result, and pass/fail against SC-001 and SC-002.

## Completion evidence

Record results for SC-001 through SC-008, including Identity/LocalDB setup, personal-data isolation, upsert timing, duplicate prevention, favorite accessibility, unavailable-source handling, mock-weather recommendation parity, anonymous regression, security/logging review, and performance measurements.
