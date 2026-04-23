using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;
using ThingsBooksy.Shared.Abstractions.Events;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Events.Handlers;

internal sealed class UserSignedUpHandler : IEventHandler<UserSignedUp>
{
    private readonly ManagementGroupsDbContext _dbContext;

    public UserSignedUpHandler(ManagementGroupsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(UserSignedUp @event, CancellationToken cancellationToken = default)
    {
        var userReadModel = new UserReadModel
        {
            Id = @event.UserId,
            Email = @event.Email.ToLowerInvariant()
        };

        await _dbContext.UserReadModels.AddAsync(userReadModel, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
