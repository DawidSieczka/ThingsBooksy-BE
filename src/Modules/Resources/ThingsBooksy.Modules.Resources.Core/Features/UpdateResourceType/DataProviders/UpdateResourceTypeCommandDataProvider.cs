using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.ReadModels;

namespace ThingsBooksy.Modules.Resources.Core.Features.UpdateResourceType;

internal sealed class UpdateResourceTypeCommandDataProvider : IUpdateResourceTypeCommandDataProvider
{
    private readonly ResourcesDbContext _dbContext;

    public UpdateResourceTypeCommandDataProvider(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public Task<ResourceType?> GetResourceTypeWithDefinitionsAsync(Guid typeId, CancellationToken ct)
        => _dbContext.ResourceTypes
            .Include(t => t.PropertyDefinitions)
            .FirstOrDefaultAsync(t => t.Id == typeId, ct);

    public Task<GroupReadModel?> GetGroupAsync(Guid groupId, CancellationToken ct)
        => _dbContext.GroupReadModels.FirstOrDefaultAsync(g => g.Id == groupId, ct);

    public void RemovePropertyDefinitions(IEnumerable<ResourcePropertyDefinition> definitions)
        => _dbContext.ResourcePropertyDefinitions.RemoveRange(definitions);

    public Task AddPropertyDefinitionAsync(ResourcePropertyDefinition definition, CancellationToken ct)
        => _dbContext.ResourcePropertyDefinitions.AddAsync(definition, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
