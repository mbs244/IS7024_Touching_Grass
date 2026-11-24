# My Plants Removal Contract

## Page interaction

- **Page**: Existing authenticated `/MyPlants` Razor Page.
- **Method**: POST to the page's `Remove` handler for the selected saved-entry ID. Retain the normal Razor Pages anti-forgery token.
- **Form data**: `id` only. Do not accept an owner ID or plant name as authoritative input.
- **Identity**: The handler derives `userId` from the authenticated principal and passes it with `id` to the service.
- **Completion**: Redirect back to `/MyPlants` using Post/Redirect/Get.

## Service operation

```csharp
Task<UserPlant?> RemoveAsync(
    string userId,
    int id,
    CancellationToken cancellationToken = default);
```

- A returned `UserPlant` means that the entry matching both `userId` and `id` was removed.
- `null` means that no such entry exists for that owner. It must not reveal or modify another user's entry.
- Cancellation is passed to the existing asynchronous persistence operation.
- Local and Azure-persisted implementations expose the same behavior; no new data fields, tables, or migrations are introduced.

## Feedback and accessibility

- On a returned entry, show a status confirmation containing its saved common name, e.g. `Toyon was removed from your plants.`
- On a `null` result, show non-success feedback; do not show a success message.
- On `DbException` or `DbUpdateException`, log the exception, redirect back to My Plants, show exactly `Something went wrong. Your plant was not removed.`, and do not show a success message. Do not convert request cancellation into this error message.
- Each removal form has a visible button name that identifies both the action and plant, e.g. `Remove Toyon`.
- Keep the existing favorite control separate and unchanged.

## Scope boundary

Removal changes only the current user's My Plants association. It does not change the home page, catalog record, another user's list, or the database schema.
