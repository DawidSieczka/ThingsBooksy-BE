using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Shared.Abstractions.Events;
using ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;

namespace ThingsBooksy.Modules.Resources.Core.Events.Handlers;

internal sealed class GroupMemberRemovedHandler : IEventHandler<GroupMemberRemoved>
{
    private readonly ResourcesDbContext _dbContext;

    public GroupMemberRemovedHandler(ResourcesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(GroupMemberRemoved @event, CancellationToken cancellationToken = default)
    {
        var readModel = await _dbContext.GroupMemberReadModels
            .FirstOrDefaultAsync(x => x.GroupId == @event.GroupId && x.UserId == @event.UserId, cancellationToken);

        if (readModel is null)
        {
            return;
        }

        _dbContext.GroupMemberReadModels.Remove(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
