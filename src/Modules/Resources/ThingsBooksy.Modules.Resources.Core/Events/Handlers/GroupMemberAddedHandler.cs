using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.ReadModels;
using ThingsBooksy.Shared.Abstractions.Events;
using ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;

namespace ThingsBooksy.Modules.Resources.Core.Events.Handlers;

internal sealed class GroupMemberAddedHandler : IEventHandler<GroupMemberAdded>
{
    private readonly ResourcesDbContext _dbContext;

    public GroupMemberAddedHandler(ResourcesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(GroupMemberAdded @event, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.GroupMemberReadModels
            .AnyAsync(x => x.GroupId == @event.GroupId && x.UserId == @event.UserId, cancellationToken);

        if (exists)
        {
            return;
        }

        var readModel = GroupMemberReadModel.Upsert(@event);
        await _dbContext.GroupMemberReadModels.AddAsync(readModel, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
