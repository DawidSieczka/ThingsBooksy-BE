using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;
using ThingsBooksy.Shared.Abstractions.Events;
using ThingsBooksy.Shared.Abstractions.Events.Users;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Events.Handlers;

internal sealed class UserSignedUpHandler : IEventHandler<UserSignedUp>
{
    private readonly ManagementGroupsDbContext _dbContext;

    public UserSignedUpHandler(ManagementGroupsDbContext dbContext)
        => _dbContext = dbContext;

    public async Task HandleAsync(UserSignedUp @event, CancellationToken cancellationToken = default)
    {
        var userReadModel = UserReadModel.Upsert(@event);
        await _dbContext.UserReadModels.AddAsync(userReadModel, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
