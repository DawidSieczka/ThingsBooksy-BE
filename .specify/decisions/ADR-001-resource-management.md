# ADR-001: Resource Management
**Status:** accepted
**Date:** 2026-05-11
**Feature:** resource-management

## Context
Group owners need a way to define what kinds of bookable items their group offers and register concrete items that members can eventually book. Without a schema for resource types and instances, there is no foundation for the booking system.

## Decision
Resources module owns two-level entity model: ResourceType (schema template) and ResourceInstance (concrete bookable item). Property schema is managed via EAV (Entity-Attribute-Value) storage, not JSON columns. Ownership and membership authorization use local read-models populated by Group events. Four new domain events from Groups module are subscribed to maintain consistency.

## Rationale
- EAV enables future OData $filter on custom property values without DDL changes
- No runtime DDL — property definitions stored as data rows only
- Event-driven read-models (Option A) decouple Resources from Groups; no IModuleClient queries
- Soft-delete on instances preserves audit trail; hard-delete on types only when no instances exist
- Two-level model separates reusable schemas (types) from concrete items (instances)

## Module breakdown
- `Resources`: owns ResourceType, ResourceInstance, ResourcePropertyDefinition, ResourcePropertyValue entities; manages local read-models (GroupReadModel, GroupMemberReadModel)
- Inter-module: `Groups` → `Resources` via `GroupCreated`, `GroupDeleted`, `GroupMemberAdded`, `GroupMemberRemoved` (IMessageBroker)

## Consequences
- Future OData filtering on custom properties requires no schema migration
- Updating property definitions when instances exist is risky and must be validated
- Soft-deleted instances need explicit ?includeDeleted=true to appear in lists
- Cascade behavior on group deletion must be defined (orphaning instances vs. cleanup)

## Amendments
<!-- append-only: never edit sections above this line after initial write -->
