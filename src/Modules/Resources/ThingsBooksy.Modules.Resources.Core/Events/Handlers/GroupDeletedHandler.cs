using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Shared.Abstractions.Events;
using ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;

namespace ThingsBooksy.Modules.Resources.Core.Events.Handlers;

internal sealed class GroupDeletedHandler : IEventHandler<GroupDeleted>
{
    private readonly ResourcesDbContext _dbContext;

    public GroupDeletedHandler(ResourcesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(GroupDeleted @event, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        await _dbContext.ResourceTypes
            .IgnoreQueryFilters()
            .Where(x => x.GroupId == @event.GroupId && x.DeletedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.DeletedAt, now)
                .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await _dbContext.ResourceInstances
            .IgnoreQueryFilters()
            .Where(x => x.GroupId == @event.GroupId && x.DeletedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.DeletedAt, now)
                .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        var groupReadModel = await _dbContext.GroupReadModels
            .FirstOrDefaultAsync(x => x.Id == @event.GroupId, cancellationToken);

        if (groupReadModel is not null)
        {
            _dbContext.GroupReadModels.Remove(groupReadModel);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
