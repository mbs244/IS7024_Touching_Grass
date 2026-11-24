# Research: Remove Plants from My Plants

## User-plant service contract

- **Decision**: Add `RemoveAsync(string userId, int id, CancellationToken cancellationToken = default)` to `IUserPlantService`, returning the removed `UserPlant` when an entry owned by that user is removed and `null` when no such entry exists. Implement the operation in both `LocalUserPlantService` and `UserPlantService`.
- **Rationale**: Both implementations already scope reads and favorite changes by `userId` and entry ID. Returning the removed snapshot lets the page show the plant's saved common name without trusting a client-supplied name or doing an extra page-level lookup. The result also distinguishes success from a stale or unauthorized ID.
- **Alternatives considered**: Returning only `bool` would require the page to fetch the plant name separately or accept a hidden plant name from the request. Deleting the plant catalog record would violate the requirement to remove only the user's saved entry.

## Local and Azure persistence behavior

- **Decision**: In the local service, find and remove the entry while holding the existing per-user lock, and return a clone of the removed record. In the Azure-persisted service, find the row using both `UserId` and the entry ID, delete that row, save through the existing context, and return the removed record. Return `null` for a missing or non-owned entry.
- **Rationale**: The local implementation already synchronizes per-user mutations and clones records returned to callers. The deployed implementation already uses `ApplicationDbContext` for `UserPlant` persistence. Keeping the same owner-scoped contract across both makes the page independent of its registered environment-specific implementation.
- **Alternatives considered**: Adding a new persistence entity, changing the existing `UserPlant` shape, or adding a migration is unnecessary; the existing user-to-plant association is the item being removed. A new storage service or package would duplicate established boundaries.

## My Plants interaction and feedback

- **Decision**: Add a separate POST handler on the existing authenticated My Plants page. It accepts only the saved entry ID, obtains the user ID from the authenticated principal, invokes `RemoveAsync`, and uses Post/Redirect/Get with page feedback. Success text uses the returned record's common name (for example, `Toyon was removed from your plants.`); a `null` result produces non-success feedback without disclosing whether another user's entry exists. For database exceptions (`DbException` or `DbUpdateException`), log the exception, redirect to My Plants with `Something went wrong. Your plant was not removed.`, and do not set a success message. Allow cancellation to propagate. Give each visible remove button text that identifies the plant, such as `Remove Toyon`. Leave the favorite action intact.
- **Rationale**: This follows the page's existing POST handler and Identity conventions, prevents client input from selecting another owner, and makes success feedback depend on an actual deletion. Narrow exception handling keeps operational failures on the page without hiding cancellation or unrelated failures. A visible plant-specific button name is also its accessible name.
- **Alternatives considered**: A GET link would make a state-changing operation reachable without a form submission. A generic button label would not identify which plant it removes. Combining removal with the favorite toggle would make two different actions ambiguous.

## Validation and tests

- **Decision**: Extend the existing xUnit suite with local-service tests for successful, missing, and cross-user removal; test the Azure service using the already referenced EF Core InMemory provider; and add authenticated Razor Pages integration coverage using the existing `WebApplicationFactory` setup. Inject a `DbUpdateException` in a page test and verify the failure redirect/message, absence of success feedback, and unchanged saved entry.
- **Rationale**: These cover the shared ownership contract, both environment-specific implementations, and the user-visible success, exception, redirect, and button behavior without adding a test package or requiring an Azure connection for the routine test suite.
- **Alternatives considered**: Testing only the local implementation would leave the Azure implementation's owner filter unverified. Introducing a separate database or test framework is outside the feature's scope.
