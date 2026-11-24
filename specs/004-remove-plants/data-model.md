# Data Model: Remove Plants from My Plants

## Existing entity: User plant

The feature removes one existing `UserPlant` association. It introduces no entity, field, relationship, index, or migration.

| Field | Type | Use and validation |
|---|---|---|
| `Id` | Integer | Identifies the selected saved entry. |
| `UserId` | String | Identifies the owner; every removal must match the authenticated user's ID. |
| `SourcePlantId` | Integer | Identifies the catalog plant and is not deleted or changed by this feature. |
| `CommonNameSnapshot` | String | Supplies the plant name in the success confirmation and accessible button name. |
| `ScientificNameSnapshot` | String | Existing saved display data; unchanged by removal. |
| `IsFavorite` | Boolean | Existing favorite state; removal is a separate action. |
| `CreatedAt`, `LastSearchedAt` | Date/time | Existing metadata; unchanged by removal. |

## Relationships and ownership

- Each `UserPlant` belongs to one user through its existing `UserId` relationship.
- A removal targets the pair of authenticated `UserId` and saved-entry `Id`; it never targets a catalog `Plant` by itself.
- Other users' associations to the same catalog plant remain unchanged.

## Removal operation

| Input/result | Rules |
|---|---|
| `userId` | Required authenticated owner ID; the page obtains it from the current principal, not the form. |
| `id` | Existing saved-entry ID supplied by the per-plant POST form. |
| Removed `UserPlant` | Returned only when the matching entry owned by `userId` was removed; its saved common name is used for confirmation. |
| `null` result | No matching entry exists for that owner; no other owner's entry is changed. |

## Lifecycle and page states

1. **Listed**: The user's existing `UserPlant` appears on My Plants with its current favorite state.
2. **Removed**: One matching association is deleted; the catalog plant and all other associations remain.
3. **Not removed**: A stale or non-owned ID produces no deletion and no success confirmation.

No schema or migration changes are required.
