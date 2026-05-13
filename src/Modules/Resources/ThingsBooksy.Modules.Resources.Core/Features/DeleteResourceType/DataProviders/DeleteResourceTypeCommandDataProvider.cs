using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.ReadModels;

namespace ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceType;

internal sealed class DeleteResourceTypeCommandDataProvider : IDeleteResourceTypeCommandDataProvider
{
    private readonly ResourcesDbContext _dbContext;

    public DeleteResourceTypeCommandDataProvider(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public Task<ResourceType?> GetResourceTypeAsync(Guid typeId, CancellationToken ct)
        => _dbContext.ResourceTypes.FirstOrDefaultAsync(t => t.Id == typeId, ct);

    public Task<GroupReadModel?> GetGroupAsync(Guid groupId, CancellationToken ct)
        => _dbContext.GroupReadModels.FirstOrDefaultAsync(g => g.Id == groupId, ct);

    public Task<bool> HasInstancesAsync(Guid typeId, CancellationToken ct)
        => _dbContext.ResourceInstances.IgnoreQueryFilters().AnyAsync(i => i.ResourceTypeId == typeId, ct);

    public void RemoveResourceType(ResourceType resourceType)
        => _dbContext.ResourceTypes.Remove(resourceType);

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
