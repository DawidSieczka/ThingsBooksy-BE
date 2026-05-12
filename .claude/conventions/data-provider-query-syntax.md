---
name: data-provider-query-syntax
description: Use parenthesized LINQ query syntax with chained materialization for joins and groupings in DataProviders and integration tests; method syntax allowed for simple single-table queries.
metadata:
  type: feedback
---

## Description
Data provider methods that return collections must use LINQ query syntax wrapped in parentheses
with the materialization call chained directly outside — `(from ... select ...).ToListAsync(ct)`.
Methods returning a single result follow the same pattern with `.FirstOrDefaultAsync(ct)`.
When the entire body is a single expression, write the method as expression-bodied (`=>`).

Use query syntax for any query involving a join, group by, or more than one data source.
Method syntax is permitted only for simple queries — single DbSet, no joins, no group by.

This rule applies to DataProvider implementations and integration tests.

## Rationale
The `var query = ...; return query.ToListAsync(ct);` pattern introduces a named local variable
that serves no purpose — it does not improve readability and creates a useless allocation of a
query object reference on the stack. Chaining directly on the parenthesized expression makes the
method a single expression, which enables expression-bodied syntax and keeps the async-eliding
rule (return Task directly, no async/await) naturally enforceable. Consistency across all data
providers also reduces cognitive overhead when reading unfamiliar code.

## Bad Example

```csharp
// WRONG — unnecessary local variable, missed expression-body opportunity
public Task<List<ResourceTypeDto>> GetAllAsync(Guid groupId, CancellationToken ct)
{
    var query = from rt in _dbContext.ResourceTypes
                where rt.GroupId == groupId
                select new ResourceTypeDto(rt.Id, rt.Name);

    return query.ToListAsync(ct);
}

// WRONG — join written in method syntax
public Task<IReadOnlyList<ResourceInstanceDto>> GetByTypeAsync(Guid typeId, CancellationToken ct)
    => _dbContext.ResourceInstances
        .Join(_dbContext.ResourceTypes,
            i => i.TypeId, t => t.Id,
            (i, t) => new ResourceInstanceDto(i.Id, t.Name, i.CreatedAt))
        .Where(...)
        .ToListAsync(ct);
```

## Good Example

```csharp
// CORRECT — parenthesized query syntax, materialization chained outside, expression-bodied
public Task<List<ResourceTypeDto>> GetAllAsync(Guid groupId, CancellationToken ct)
    => (from rt in _dbContext.ResourceTypes
        where rt.GroupId == groupId
        select new ResourceTypeDto(rt.Id, rt.Name))
        .ToListAsync(ct);

// CORRECT — join uses query syntax
public Task<IReadOnlyList<ResourceInstanceDto>> GetByTypeAsync(Guid typeId, CancellationToken ct)
    => (from i in _dbContext.ResourceInstances
        join t in _dbContext.ResourceTypes on i.TypeId equals t.Id
        where i.TypeId == typeId
        select new ResourceInstanceDto(i.Id, t.Name, i.CreatedAt))
        .ToListAsync(ct);

// CORRECT — group by uses query syntax
public Task<IReadOnlyList<ResourceCountByTypeDto>> GetCountsAsync(CancellationToken ct)
    => (from i in _dbContext.ResourceInstances
        group i by i.TypeId into g
        select new ResourceCountByTypeDto(g.Key, g.Count()))
        .ToListAsync(ct);

// CORRECT — simple single-table lookup; method syntax acceptable
public Task<bool> ExistsAsync(Guid typeId, CancellationToken ct)
    => _dbContext.ResourceTypes.AnyAsync(x => x.Id == typeId, ct);
```
