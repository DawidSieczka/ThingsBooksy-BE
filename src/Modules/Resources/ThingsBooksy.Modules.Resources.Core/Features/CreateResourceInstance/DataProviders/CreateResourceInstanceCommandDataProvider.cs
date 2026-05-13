using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.ReadModels;

namespace ThingsBooksy.Modules.Resources.Core.Features.CreateResourceInstance;

internal sealed class CreateResourceInstanceCommandDataProvider : ICreateResourceInstanceCommandDataProvider
{
    private readonly ResourcesDbContext _dbContext;

    public CreateResourceInstanceCommandDataProvider(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public Task<ResourceType?> GetResourceTypeAsync(Guid resourceTypeId, CancellationToken ct)
        => _dbContext.ResourceTypes.FirstOrDefaultAsync(t => t.Id == resourceTypeId, ct);

    public Task<GroupReadModel?> GetGroupAsync(Guid groupId, CancellationToken ct)
        => _dbContext.GroupReadModels.FirstOrDefaultAsync(g => g.Id == groupId, ct);

    public Task<bool> NameExistsAsync(Guid resourceTypeId, string name, CancellationToken ct)
        => _dbContext.ResourceInstances.AnyAsync(x => x.ResourceTypeId == resourceTypeId && x.Name == name, ct);

    public Task<List<ResourcePropertyDefinition>> GetPropertyDefinitionsAsync(Guid resourceTypeId, CancellationToken ct)
        => _dbContext.ResourcePropertyDefinitions.Where(d => d.ResourceTypeId == resourceTypeId).ToListAsync(ct);

    public Task AddResourceInstanceAsync(ResourceInstance instance, CancellationToken ct)
        => _dbContext.ResourceInstances.AddAsync(instance, ct).AsTask();

    public Task AddPropertyValueAsync(ResourcePropertyValue propertyValue, CancellationToken ct)
        => _dbContext.ResourcePropertyValues.AddAsync(propertyValue, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
