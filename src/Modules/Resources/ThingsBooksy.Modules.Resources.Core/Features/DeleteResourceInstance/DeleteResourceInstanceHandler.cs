using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceInstance;

internal sealed class DeleteResourceInstanceHandler : ICommandHandler<DeleteResourceInstanceCommand>
{
    private readonly ResourcesDbContext _dbContext;
    private readonly IClock _clock;

    public DeleteResourceInstanceHandler(ResourcesDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(DeleteResourceInstanceCommand command, CancellationToken cancellationToken = default)
    {
        var instance = await _dbContext.ResourceInstances
            .FirstOrDefaultAsync(x => x.Id == command.InstanceId, cancellationToken);

        if (instance is null)
            throw new ResourcesDomainException("Resource instance not found.");

        var isOwner = await _dbContext.GroupReadModels
            .AnyAsync(g => g.Id == instance.GroupId && g.OwnerId == command.RequesterId, cancellationToken);

        if (!isOwner)
            throw new ResourcesForbiddenException("Only the group owner may delete a resource instance.");

        instance.Delete(_clock.CurrentDate());

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
