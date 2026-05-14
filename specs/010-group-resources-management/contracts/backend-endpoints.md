# Backend HTTP Contracts — feature 010

All endpoints listed below are part of feature 010. Endpoints not listed are reused as-is and out of scope of this contract document.

Conventions:
- `200 OK` for successful reads, `201 Created` (with `Location`) for successful entity creation, `204 No Content` for void mutations, `400 Bad Request` for validation failures, `401 Unauthorized` (handled by middleware) when JWT missing/invalid, `403 Forbidden` when caller lacks role, `404 Not Found` when target entity missing, `409 Conflict` for unique-constraint collisions.
- All endpoints require Bearer JWT unless explicitly noted.
- All identifiers are `Guid` v7. Date-times are `DateTimeOffset` serialised as ISO-8601.

---

## ManagementGroups module

### `GET /management-groups/name-available` (NEW)

Check whether a group name is available for the authenticated user (case-insensitive trimmed comparison, scoped to caller as owner; soft-deleted groups don't count).

**Request**
| Param | Location | Type | Required | Notes |
|---|---|---|---|---|
| `name` | query | string | yes | Length 1–100, leading/trailing whitespace trimmed before check. |

**Responses**
- `200 OK` `application/json`
  ```json
  { "available": true }
  ```
- `400 Bad Request` — empty/whitespace/too long.

**Authorization**: Authenticated user.
**Side effects**: None.

---

### `GET /management-groups/{id}/members` (NEW)

Paginated members list. First page always includes the owner as the first entry with `isOwner: true`.

**Request**
| Param | Location | Type | Required | Notes |
|---|---|---|---|---|
| `id` | path | Guid | yes | Group id. |
| `afterId` | query | Guid | no | Cursor — UserId of the last item from previous page. Omit on first page. |
| `take` | query | int | no | Default 20, clamped to `[1, 50]`. |

**Responses**
- `200 OK` `application/json`
  ```json
  {
    "items": [
      { "userId": "guid", "email": "owner@example.com", "joinedAt": "iso8601", "isOwner": true  },
      { "userId": "guid", "email": "user1@example.com", "joinedAt": "iso8601", "isOwner": false }
    ],
    "nextCursor": "guid-or-null"
  }
  ```
- `403 Forbidden` — caller is neither owner nor member.
- `404 Not Found` — group doesn't exist or is soft-deleted.

**Authorization**: Owner OR member of the group.
**Side effects**: None.

---

### `POST /management-groups` (AMEND — 409 added)

Existing endpoint. Behaviour unchanged except for one new error path.

**New error**
- `409 Conflict` `application/json`
  ```json
  { "code": "GROUP_NAME_TAKEN", "message": "You already own a group with this name." }
  ```

**Trigger**: Submitting a `name` equal (case-insensitive trimmed) to an existing non-deleted group owned by the caller.

---

### `PUT /management-groups/{id}` (AMEND — 409 added)

Same as `POST /management-groups` 409 semantics, evaluated against the caller's other non-deleted groups (the group being updated is excluded from the check).

---

## Resources module

### `GET /resources/instances` (AMEND — cursor pagination, response shape change)

Cursor-paginated list of resource instances in a group.

**Request**
| Param | Location | Type | Required | Notes |
|---|---|---|---|---|
| `groupId` | query | Guid | yes (was already required) | Filters by group. |
| `resourceTypeId` | query | Guid | no | Optional schema filter. |
| `afterId` | query | Guid | no | Cursor. |
| `take` | query | int | no | Default 20, clamped `[1, 50]`. |
| `includeDeleted` | query | bool | no | Default false. |

**Response shape change** — **breaking**: was `T[]`, now `{ items: T[], nextCursor: Guid? }`. The FE client is regenerated, no other callers.

**Responses**
- `200 OK` `application/json`
  ```json
  {
    "items": [
      {
        "id": "guid",
        "resourceTypeId": "guid",
        "groupId": "guid",
        "name": "string",
        "description": "string|null",
        "createdAt": "iso8601",
        "propertyValues": [
          { "propertyDefinitionId": "guid", "value": "string" }
        ]
      }
    ],
    "nextCursor": "guid-or-null"
  }
  ```
- `403 Forbidden` — caller not member/owner of group.
- `404 Not Found` — group doesn't exist.

**Authorization**: Owner OR member of the group.

---

### `POST /resources/types` (AMEND — 409 added)

**New error**
- `409 Conflict` `application/json`
  ```json
  { "code": "RESOURCE_TYPE_NAME_TAKEN", "message": "A schema with this name already exists in the group." }
  ```

**Trigger**: Submitting a `name` equal (case-insensitive trimmed) to an existing schema in the same group.

---

### `PUT /resources/types/{id}` (AMEND — 409 added)

Same as `POST /resources/types` 409 semantics, evaluated against other schemas in the same group (the schema being updated excluded).

---

## Cross-cutting

### HTTP response for soft-deleted vs missing

- Group soft-deleted ⇒ `404 Not Found` (treated as missing for non-owner; the owner sees it gone from list, can only `POST /restore` from a future iteration's UI).
- Schema cascade-deleted via `GroupDeleted` event ⇒ subsequent `GET /resources/types/{id}` returns `404`.

### Error envelope

All non-2xx responses with a body use the shape:
```json
{ "code": "MACHINE_READABLE_CODE", "message": "Human readable message", "detail": "optional extra info" }
```

The `errorInterceptor` on the FE uses `message` for the toast text when present, falling back to the HTTP statusText.

---

## Endpoints unaffected by this feature

The following are reused verbatim and remain in their current shape:

- `GET /management-groups` — returns user's groups (flat list, no pagination this iteration).
- `GET /management-groups/{id}` — returns single group with `MemberCount` (new) and inline `Members` (first page only — owner first, then up to first 20 members).
- `DELETE /management-groups/{id}` — soft-deletes group AND publishes `GroupDeleted` event (existing behaviour, used by cascade).
- `POST /management-groups/{id}/restore`, `POST/DELETE /management-groups/{id}/members/...` — unchanged.
- `GET/POST/PUT/DELETE /resources/types/{id}` — listing one type, updating, deleting (rest unchanged besides 409 addition above).
- `POST /resources/instances`, `GET /resources/instances/{id}`, `PUT /resources/instances/{id}`, `DELETE /resources/instances/{id}` — unchanged.
