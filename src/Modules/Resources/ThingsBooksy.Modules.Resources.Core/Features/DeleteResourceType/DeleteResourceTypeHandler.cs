using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceType;

internal sealed class DeleteResourceTypeHandler : ICommandHandler<DeleteResourceTypeCommand>
{
    private readonly ResourcesDbContext _dbContext;

    public DeleteResourceTypeHandler(ResourcesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(DeleteResourceTypeCommand command, CancellationToken cancellationToken = default)
    {
        var resourceType = await _dbContext.ResourceTypes
            .FirstOrDefaultAsync(t => t.Id == command.TypeId, cancellationToken);

        if (resourceType is null)
            throw new ResourcesDomainException("Resource type not found.");

        var group = await _dbContext.GroupReadModels
            .FirstOrDefaultAsync(g => g.Id == resourceType.GroupId, cancellationToken);

        if (group is null || group.OwnerId != command.RequesterId)
            throw new ResourcesForbiddenException("Only the group owner may delete a resource type.");

        var hasInstances = await _dbContext.ResourceInstances
            .IgnoreQueryFilters()
            .AnyAsync(i => i.ResourceTypeId == command.TypeId, cancellationToken);

        if (hasInstances)
            throw new ResourcesDomainException("Cannot delete a resource type that has instances.");

        _dbContext.ResourceTypes.Remove(resourceType);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
