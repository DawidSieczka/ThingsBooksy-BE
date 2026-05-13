using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Domain;

namespace ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceInstance;

internal sealed class DeleteResourceInstanceCommandDataProvider : IDeleteResourceInstanceCommandDataProvider
{
    private readonly ResourcesDbContext _dbContext;

    public DeleteResourceInstanceCommandDataProvider(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public Task<ResourceInstance?> GetInstanceAsync(Guid instanceId, CancellationToken ct)
        => _dbContext.ResourceInstances.FirstOrDefaultAsync(x => x.Id == instanceId, ct);

    public Task<bool> IsOwnerAsync(Guid groupId, Guid userId, CancellationToken ct)
        => _dbContext.GroupReadModels.AnyAsync(g => g.Id == groupId && g.OwnerId == userId, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
