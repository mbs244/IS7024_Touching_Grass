# Quickstart: Plant Search

## Prerequisites

- .NET 10 SDK
- Repository restored using its existing project dependencies

## Automated tests

From the repository root, run:

```powershell
dotnet test .\tests\Touching_Grass.UnitTests\Touching_Grass.UnitTests.csproj
```

The search tests should cover partial matching in both name fields, case insensitivity, whitespace trimming, empty input, no matches, duplicate prevention when both fields match, cancellation propagation where practical, and the 50-result limit with catalog ordering.

## Run and verify the page

Start the application:

```powershell
dotnet run --project .\Touching_Grass\Touching_Grass.csproj
```

Open the application URL printed by the command and select **Plants** in the navigation. Verify:

1. Search for `salvia`; white sage appears in the results. Each row has common name, scientific name, and water factor.
2. Search for `TOYON`; toyon appears.
3. Search for `sklujapouetllkjsda`; there are no plant rows and the exact message `This plant is not in our supported plant database.` appears.
4. Submit an empty search; no catalog-wide results or no-match message appears.
5. Search for a broad term with more than 50 matches; no more than the first 50 catalog-order matches appear.
6. Confirm the shared Plants navigation link works and the home page content remains unchanged.

See [the page contract](./contracts/plants-page.md) for the UI behavior and [the data model](./data-model.md) for search/result rules.
