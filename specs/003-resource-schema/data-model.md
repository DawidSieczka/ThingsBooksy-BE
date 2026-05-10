# Data Model: Resource Schema Management

**Branch**: `003-resource-schema`

## Entities

### ResourceType

Represents a category of bookable items within a management group. Defines the schema template for all instances of that type.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | GUID v7, PK |
| GroupId | Guid | FK reference to group (from GroupReadModel) |
| Name | string | Required, non-empty |
| Description | string? | Optional |
| CreatedAt | DateTime | Set on Create |
| UpdatedAt | DateTime | Set on Create and Update |
| DeletedAt | DateTime? | Null = active; set on Delete |

**Navigation**: `ICollection<ResourcePropertyDefinition> PropertyDefinitions`

**Domain methods**:
- `static ResourceType Create(Guid id, Guid groupId, string name, string? description, DateTime now)`
- `void Update(string name, string? description, DateTime now)`
- `void Delete(DateTime now)`
- `bool IsDeleted => DeletedAt.HasValue`

---

### ResourcePropertyDefinition

A single field definition belonging to a ResourceType. Defines the name, data type, and required flag for one property.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | GUID v7, PK |
| ResourceTypeId | Guid | FK → ResourceType |
| Name | string | Required, non-empty |
| DataType | PropertyDataType | Enum: Text, Number, Boolean |
| IsRequired | bool | Enforced on instance create/update |

**Domain methods**:
- `static ResourcePropertyDefinition Create(Guid id, Guid resourceTypeId, string name, PropertyDataType dataType, bool isRequired)`
- `void Update(string name, PropertyDataType dataType, bool isRequired)`

---

### ResourceInstance

A concrete, bookable item of a given ResourceType. Carries property values matching the type's schema.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | GUID v7, PK |
| ResourceTypeId | Guid | FK → ResourceType |
| GroupId | Guid | Denormalised for query convenience |
| Name | string | Required, non-empty |
| OwnerId | Guid | User who created it (from JWT on creation) |
| CreatedAt | DateTime | Set on Create |
| UpdatedAt | DateTime | Set on Create and Update |
| DeletedAt | DateTime? | Null = active; set on Delete (soft-delete) |

**Navigation**: `ICollection<ResourcePropertyValue> PropertyValues`

**Domain methods**:
- `static ResourceInstance Create(Guid id, Guid resourceTypeId, Guid groupId, string name, Guid ownerId, DateTime now)`
- `void Update(string name, DateTime now)`
- `void Delete(DateTime now)`
- `bool IsDeleted => DeletedAt.HasValue`

---

### ResourcePropertyValue

EAV value row: one property value for one resource instance.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | GUID v7, PK |
| ResourceInstanceId | Guid | FK → ResourceInstance |
| PropertyDefinitionId | Guid | FK → ResourcePropertyDefinition |
| Value | string | Always stored as text; cast on read per DataType |

**Domain methods**:
- `static ResourcePropertyValue Create(Guid id, Guid resourceInstanceId, Guid propertyDefinitionId, string value)`
- `void Update(string value)`

---

### GroupReadModel (local read-model)

Local copy of ManagementGroup data, kept in sync via integration events.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | PK, matches ManagementGroup.Id |
| OwnerId | Guid | Used for ownership authorisation |

**Not a domain entity** — no private constructor, no domain methods. Plain class with public setters (EF pattern, same as `UserReadModel`).

---

### GroupMemberReadModel (local read-model)

Local copy of group membership, kept in sync via integration events.

| Field | Type | Notes |
|---|---|---|
| GroupId | Guid | Part of composite PK |
| UserId | Guid | Part of composite PK |

**Not a domain entity** — plain class with public setters.

---

## Enum

### PropertyDataType

```
Text    = 0
Number  = 1
Boolean = 2
```

Stored as string in EF (`HasConversion<string>()`).

---

## Relationships

```
ManagementGroup (GroupReadModel)
    └── ResourceType (many)
            └── ResourcePropertyDefinition (many)
            └── ResourceInstance (many)
                    └── ResourcePropertyValue (many)
                            └── ResourcePropertyDefinition (FK, for join)

GroupMemberReadModel: (GroupId, UserId) composite PK
```

---

## EF Schema: `resources`

Tables:
- `resource_types` — ResourceType
- `resource_property_definitions` — ResourcePropertyDefinition
- `resource_instances` — ResourceInstance
- `resource_property_values` — ResourcePropertyValue
- `group_read_models` — GroupReadModel
- `group_member_read_models` — GroupMemberReadModel
- `Inbox` / `Outbox` — already present in scaffold

---

## Validation Rules

| Scenario | Rule |
|---|---|
| Create ResourceType | Name non-empty; at least one property definition; GroupId exists in GroupReadModel; caller == GroupReadModel.OwnerId |
| Create ResourceInstance | Name non-empty; ResourceTypeId exists; GroupId matches type's group; caller == group owner; all required definitions have values; value matches DataType |
| Update ResourceType | Same owner check; name non-empty |
| Update ResourceInstance | Same owner check; name non-empty; required values present; types valid |
| Delete ResourceType | Caller == owner; zero instances exist (including soft-deleted) |
| Delete ResourceInstance | Caller == owner; soft-delete only |

---

## Integration Events (new — Shared.Abstractions)

Located in `Events/ManagementGroups/`:

```csharp
public record GroupCreated(Guid GroupId, Guid OwnerId) : IEvent;
public record GroupDeleted(Guid GroupId) : IEvent;
public record GroupMemberAdded(Guid GroupId, Guid UserId) : IEvent;
public record GroupMemberRemoved(Guid GroupId, Guid UserId) : IEvent;
```
