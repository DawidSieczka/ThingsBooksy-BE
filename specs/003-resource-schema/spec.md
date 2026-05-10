# Feature Specification: Resource Schema Management

**Feature Branch**: `003-resource-schema`
**Created**: 2026-05-11
**Status**: Draft
**Input**: User description: "Resources module — generic bookable resource schema. Two-level model: ResourceType (schema template with dynamic PropertyDefinitions) → ResourceInstance (concrete bookable item with EAV PropertyValues). Each ResourceType belongs to a ManagementGroup and defines custom fields (Text/Number/Boolean, required or nullable). EAV storage: ResourcePropertyDefinitions + ResourcePropertyValues tables (fixed schema, runtime rows only). Authorization: group owner only for writes. Group ownership/membership verified via local read-models populated by four new events from the Groups module: GroupCreated, GroupDeleted, GroupMemberAdded, GroupMemberRemoved. Services and booking/reservation logic are out of scope. Module scaffold at src/Modules/Resources/ already exists."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Define a Resource Type (Priority: P1)

A management group owner wants to register a new category of bookable things for their group. For example, a fleet manager creates a "Car" resource type and specifies that every car must have a "Power (HP)" field (number, required) and optionally an "Average Fuel Consumption" field (text, optional). A different group owner creates a "Meeting Room" type with "Seats" (number, required), "Ethernet Access" (boolean, required), and "Projector" (boolean, required).

**Why this priority**: Without a resource type, there is nothing to create instances of. This is the foundational piece the entire feature depends on.

**Independent Test**: Can be tested by creating a resource type via the API and verifying the type and its property definitions are persisted correctly.

**Acceptance Scenarios**:

1. **Given** an authenticated group owner, **When** they submit a new resource type with a name, an optional description, and at least one property definition, **Then** the system creates the resource type and returns its ID.
2. **Given** a resource type creation request, **When** a property definition has no name or an unrecognised data type, **Then** the system rejects the request with a validation error identifying the invalid field.
3. **Given** an authenticated user who is not the group owner, **When** they attempt to create a resource type, **Then** the system returns 403 Forbidden.
4. **Given** an unauthenticated request, **When** it attempts to create a resource type, **Then** the system returns 401 Unauthorized.

---

### User Story 2 - Add Resource Instances of a Type (Priority: P2)

A group owner has already defined a "Car" resource type. They now register concrete cars: "Car 1" with Power = 116, Fuel Consumption = "3.8–4.4 l/100 km", and "Car 2" with Power = 130, Fuel Consumption = "5.5–6.0 l/100 km". Each instance carries the values defined by the type's schema.

**Why this priority**: Instances are what group members will eventually book. Once the type exists, owners need to register the actual physical items.

**Independent Test**: Can be tested end-to-end by first creating a type, then creating two instances with valid property values, and verifying all values are retrievable.

**Acceptance Scenarios**:

1. **Given** a valid resource type with required and optional property definitions, **When** an owner submits a new instance with all required values present, **Then** the system creates the instance and returns its ID.
2. **Given** a resource type with a required property, **When** an owner submits an instance without that required property value, **Then** the system returns 400 Bad Request identifying the missing property.
3. **Given** a resource type with a Number property, **When** an owner submits an instance with a non-numeric value for that property, **Then** the system returns 400 Bad Request.
4. **Given** a valid instance creation request, **When** the caller is not the group owner, **Then** the system returns 403 Forbidden.

---

### User Story 3 - View Resource Types and Instances (Priority: P2)

Any authenticated group member can browse the resource types defined for their group and see the instances of each type, along with all their property values. For example, a member can list all "Car" instances for their group to see which cars are available and what specs each has.

**Why this priority**: Reading resource data is required for any future booking flow and for members to know what the group owns.

**Independent Test**: Can be tested by retrieving types and instances via GET endpoints and verifying the response includes property definitions (for types) and property values (for instances).

**Acceptance Scenarios**:

1. **Given** a group with two resource types, **When** a group member calls GET /resources/types?groupId={id}, **Then** the system returns both types with their property definitions.
2. **Given** a resource type with three instances, **When** a member calls GET /resources/instances?resourceTypeId={id}, **Then** the system returns all three instances with their property values.
3. **Given** a user who is not a member of the group, **When** they request the group's resource types or instances, **Then** the system returns 403 Forbidden.
4. **Given** a request for a resource instance that has been soft-deleted, **When** the caller does not pass includeDeleted=true, **Then** the system excludes the deleted instance from the response.

---

### User Story 4 - Update and Remove Resources (Priority: P3)

A group owner renames a resource instance ("Car 1" → "Company Hatchback") and updates its fuel consumption value. Later, they soft-delete a car that has been taken out of service. The car remains in the system but is hidden from default listings.

**Why this priority**: Lifecycle management keeps the resource catalogue accurate over time without permanently losing historical data.

**Independent Test**: Can be tested by updating an instance's name and property values, then deleting it and verifying it no longer appears in default listings but is retrievable with ?includeDeleted=true.

**Acceptance Scenarios**:

1. **Given** an existing instance, **When** the owner submits updated name and property values, **Then** the system persists the changes and returns 204.
2. **Given** an existing instance, **When** the owner sends a DELETE request, **Then** the system soft-deletes it (sets a deletion timestamp) and returns 204.
3. **Given** a soft-deleted instance, **When** a member lists instances without the includeDeleted flag, **Then** the deleted instance does not appear.
4. **Given** a soft-deleted instance, **When** a member lists instances with ?includeDeleted=true, **Then** the deleted instance appears with its deletion timestamp visible.
5. **Given** a non-owner member, **When** they attempt to update or delete an instance, **Then** the system returns 403 Forbidden.

---

### User Story 5 - Update and Remove Resource Types (Priority: P3)

A group owner renames a resource type and adds a new optional property definition to the schema. They can also delete a resource type, but only if it has no instances — this prevents orphaned data.

**Why this priority**: Keeping the schema accurate matters, but type updates are less frequent than instance operations.

**Independent Test**: Can be tested by updating a type's name and adding a new property definition, then verifying instances can still be created with the updated schema.

**Acceptance Scenarios**:

1. **Given** an existing resource type, **When** the owner updates its name and adds a new optional property definition, **Then** the changes are persisted and existing instances are unaffected.
2. **Given** a resource type with no instances, **When** the owner deletes it, **Then** the system removes it and returns 204.
3. **Given** a resource type that has one or more instances, **When** the owner attempts to delete it, **Then** the system returns 400 Bad Request (type has instances).

---

### Edge Cases

- What happens when a group is deleted — are its resource types and instances also removed or orphaned?
- What happens if an owner changes the DataType of an existing property definition when instances with values already exist for that definition?
- What happens when a user is removed from a group — can they still see instances they previously could access?
- What happens when a property definition is removed from a type update while instances have existing values for that definition?
- What happens if a value string for a Number property cannot be parsed as a number on read?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow authenticated group owners to create resource types with a name, optional description, group ID, and one or more property definitions.
- **FR-002**: Each property definition MUST have a name, a data type (Text, Number, or Boolean), and a required/optional flag.
- **FR-003**: The system MUST allow authenticated group owners to create resource instances linked to an existing resource type, providing values for each property definition.
- **FR-004**: The system MUST enforce that all required property definitions on a resource type have a corresponding value when creating or updating an instance.
- **FR-005**: The system MUST validate that property values are compatible with the declared data type of their definition (e.g., value for a Number property must be parseable as a number).
- **FR-006**: The system MUST allow authenticated group owners to update resource type names, descriptions, and property definitions.
- **FR-007**: The system MUST allow authenticated group owners to update resource instance names and property values.
- **FR-008**: The system MUST allow authenticated group owners to soft-delete resource instances (set a deletion timestamp); instances are never physically deleted.
- **FR-009**: The system MUST allow authenticated group owners to hard-delete resource types only when the type has no associated instances.
- **FR-010**: The system MUST return all non-deleted resource types for a group when queried by group ID.
- **FR-011**: The system MUST return all non-deleted resource instances for a type (or group) by default; deleted instances MUST only appear when an explicit opt-in parameter is passed.
- **FR-012**: The system MUST restrict write operations (create, update, delete) on resource types and instances to the group owner only.
- **FR-013**: The system MUST restrict read operations on resource types and instances to authenticated members of the owning group.
- **FR-014**: The system MUST maintain a local read-model of management groups (group ID and owner ID) by subscribing to group lifecycle events from the Groups module.
- **FR-015**: The system MUST maintain a local read-model of group memberships (group ID and user ID) by subscribing to group membership events from the Groups module.
- **FR-016**: The system MUST reject resource type or instance creation requests that reference a group ID not present in the local group read-model.

### Key Entities

- **ResourceType**: Represents a category of bookable items within a management group. Has a name, optional description, and belongs to exactly one group. Defines the schema that all its instances must conform to.
- **ResourcePropertyDefinition**: A single field definition on a ResourceType. Has a name, a data type (Text, Number, Boolean), and an isRequired flag. There can be many definitions per type.
- **ResourceInstance**: A concrete, bookable item of a given ResourceType. Has a name, belongs to a group and a type, is owned by the group owner who created it, and carries a set of property values matching the type's definitions. Supports soft-delete via a deletion timestamp.
- **ResourcePropertyValue**: The actual value of one property definition for one resource instance, stored as text and cast on read according to the definition's data type.
- **GroupReadModel** (local): A local copy of each management group (ID and owner ID), kept in sync via subscribed events. Used to authorise write operations.
- **GroupMemberReadModel** (local): A local copy of group memberships (group ID and user ID), kept in sync via subscribed events. Used to authorise read operations and instance creation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A group owner can define a new resource type with custom property fields in under 2 minutes using only the API.
- **SC-002**: A group owner can register a new resource instance with all required property values in under 1 minute.
- **SC-003**: Group members can retrieve all resource types and instances for their group with property values in a single request per entity type.
- **SC-004**: All write operations that violate ownership rules are rejected — no unauthorised user can create, update, or delete a resource type or instance.
- **SC-005**: Soft-deleted instances do not appear in default listings, ensuring members only see active resources without additional filtering effort.
- **SC-006**: The property schema is fully generic — any combination of Text, Number, and Boolean fields can be defined without application code changes.

## Assumptions

- The ManagementGroups module will be updated to publish four new events: GroupCreated, GroupDeleted, GroupMemberAdded, GroupMemberRemoved. These events do not currently exist.
- The Resources module scaffold (DbContext, module registration, extensions) already exists at `src/Modules/Resources/` but contains no domain entities.
- Services (hairdresser, auto repair) are explicitly out of scope for this feature. Only physical/bookable objects are in scope.
- Booking and reservation logic (who books what, when, for how long) is out of scope. This feature covers schema definition and resource management only.
- The group owner at the time of instance creation is recorded as the instance owner. Owner transfer is out of scope.
- Property values for Number fields are stored as text and validated on write; cast errors on read are treated as data integrity issues outside this feature's scope.
- What happens to resource types and instances when a group is deleted is an open question deferred to implementation (cascade soft-delete is the safest default).
- Changing a property definition's DataType on an existing type when instances already have values is assumed to be allowed for now; values remain as-is and are re-interpreted on read.
- The Resources module schema name for the database is `resources`.
- Authentication uses JWT Bearer tokens consistent with the rest of the platform; the caller's user ID is extracted from the token.
