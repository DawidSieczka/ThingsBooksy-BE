using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.ReadModels;
using ThingsBooksy.Shared.Abstractions.Events;
using ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;

namespace ThingsBooksy.Modules.Resources.Core.Events.Handlers;

internal sealed class GroupCreatedHandler : IEventHandler<GroupCreated>
{
    private readonly ResourcesDbContext _dbContext;

    public GroupCreatedHandler(ResourcesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(GroupCreated @event, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.GroupReadModels
            .FirstOrDefaultAsync(x => x.Id == @event.GroupId, cancellationToken);

        if (existing is not null)
        {
            existing.OwnerId = @event.OwnerId;
        }
        else
        {
            var readModel = new GroupReadModel
            {
                Id = @event.GroupId,
                OwnerId = @event.OwnerId
            };
            await _dbContext.GroupReadModels.AddAsync(readModel, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
