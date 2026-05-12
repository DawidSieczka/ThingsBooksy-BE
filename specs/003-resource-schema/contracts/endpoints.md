# API Contracts: Resource Schema Management

**Module route prefix**: `/resources`
**Authentication**: JWT Bearer required on all endpoints

---

## Resource Types

### POST /resources/types

Create a new resource type for a management group.

**Request body**:
```json
{
  "groupId": "guid",
  "name": "string",
  "description": "string | null",
  "propertyDefinitions": [
    {
      "name": "string",
      "dataType": "Text | Number | Boolean",
      "isRequired": true
    }
  ]
}
```

**Responses**:
- `201 Created` — `{ "id": "guid" }`
- `400 Bad Request` — validation error (name empty, invalid dataType, group not found)
- `403 Forbidden` — caller is not the group owner
- `401 Unauthorized`

---

### GET /resources/types/{id}

Get a resource type by ID including all property definitions.

**Responses**:
- `200 OK` — ResourceTypeDto (see below)
- `404 Not Found`
- `403 Forbidden` — caller is not a group member
- `401 Unauthorized`

---

### GET /resources/types?groupId={groupId}

List all non-deleted resource types for a group.

**Query params**: `groupId` (required)

**Responses**:
- `200 OK` — `ResourceTypeDto[]`
- `403 Forbidden` — caller not a group member
- `401 Unauthorized`

---

### PUT /resources/types/{id}

Update a resource type's name, description, and property definitions.

For `propertyDefinitions` in the update:
- Entry with `id` present → update existing definition
- Entry without `id` → add new definition
- Definitions omitted from the array → removed from the type

**Request body**:
```json
{
  "name": "string",
  "description": "string | null",
  "propertyDefinitions": [
    {
      "id": "guid | null",
      "name": "string",
      "dataType": "Text | Number | Boolean",
      "isRequired": true
    }
  ]
}
```

**Responses**:
- `204 No Content`
- `400 Bad Request`
- `403 Forbidden`
- `404 Not Found`
- `401 Unauthorized`

---

### DELETE /resources/types/{id}

Hard-delete a resource type. Only allowed when the type has no instances (including soft-deleted).

**Responses**:
- `204 No Content`
- `400 Bad Request` — type has instances
- `403 Forbidden`
- `404 Not Found`
- `401 Unauthorized`

---

## Resource Instances

### POST /resources/instances

Create a new resource instance of a given type.

**Request body**:
```json
{
  "resourceTypeId": "guid",
  "groupId": "guid",
  "name": "string",
  "propertyValues": [
    {
      "propertyDefinitionId": "guid",
      "value": "string"
    }
  ]
}
```

**Responses**:
- `201 Created` — `{ "id": "guid" }`
- `400 Bad Request` — required property missing, value type mismatch, type not found
- `403 Forbidden`
- `401 Unauthorized`

---

### GET /resources/instances/{id}

Get a resource instance by ID including all property values.

**Responses**:
- `200 OK` — ResourceInstanceDto (see below)
- `404 Not Found`
- `403 Forbidden`
- `401 Unauthorized`

---

### GET /resources/instances?resourceTypeId={id}&groupId={id}&includeDeleted=false

List resource instances. At least one of `resourceTypeId` or `groupId` must be provided.

**Responses**:
- `200 OK` — `ResourceInstanceDto[]`
- `403 Forbidden`
- `401 Unauthorized`

---

### PUT /resources/instances/{id}

Update a resource instance's name and property values.

**Request body**:
```json
{
  "name": "string",
  "propertyValues": [
    {
      "propertyDefinitionId": "guid",
      "value": "string"
    }
  ]
}
```

**Responses**:
- `204 No Content`
- `400 Bad Request`
- `403 Forbidden`
- `404 Not Found`
- `401 Unauthorized`

---

### DELETE /resources/instances/{id}

Soft-delete a resource instance (sets DeletedAt).

**Responses**:
- `204 No Content`
- `403 Forbidden`
- `404 Not Found`
- `401 Unauthorized`

---

## DTOs

### ResourceTypeDto
```json
{
  "id": "guid",
  "groupId": "guid",
  "name": "string",
  "description": "string | null",
  "createdAt": "datetime",
  "updatedAt": "datetime",
  "propertyDefinitions": [
    {
      "id": "guid",
      "name": "string",
      "dataType": "Text | Number | Boolean",
      "isRequired": true
    }
  ]
}
```

### ResourceInstanceDto
```json
{
  "id": "guid",
  "resourceTypeId": "guid",
  "groupId": "guid",
  "name": "string",
  "ownerId": "guid",
  "createdAt": "datetime",
  "updatedAt": "datetime",
  "deletedAt": "datetime | null",
  "propertyValues": [
    {
      "propertyDefinitionId": "guid",
      "propertyName": "string",
      "dataType": "Text | Number | Boolean",
      "value": "string"
    }
  ]
}
```
