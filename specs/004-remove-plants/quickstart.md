# Quickstart: Remove Plants from My Plants

## Prerequisites

- .NET 10 SDK
- Repository restored with its existing dependencies
- For the Azure-hosted check, an existing test deployment with its normal protected MySQL connection setting and schema; this feature adds no migration

## Automated validation

Run the focused test project:

```powershell
dotnet test tests\Touching_Grass.UnitTests\Touching_Grass.UnitTests.csproj
```

The tests should cover both service implementations, owner isolation, missing entries, and the authenticated My Plants form/confirmation flow.

## Local end-to-end check

1. Start the application in Development mode:

   ```powershell
   dotnet run --project Touching_Grass\Touching_Grass.csproj
   ```

2. Register or sign in, add a plant through the existing recommendation flow, and open `/MyPlants`.
3. Confirm each row has a separate Remove button whose accessible name includes that plant's name.
4. Activate a plant's Remove button. Confirm the row disappears and a status message names the removed plant.
5. Confirm the plant remains in the plant catalog, the favorite action for other entries still works, and the home page is unchanged.
6. With a second test account, confirm its list remains unchanged. Submit a stale or non-owned entry ID and confirm no entry is removed and no success message appears.

## Azure-hosted check

Repeat the end-to-end scenarios using a test account on the existing Azure-hosted deployment. Verify that successful removal and owner isolation match the local behavior. No database update or migration should be run for this feature.
