using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceType;

internal sealed class DeleteResourceTypeCommandHandler : ICommandHandler<DeleteResourceTypeCommand>
{
    private readonly IDeleteResourceTypeCommandDataProvider _dataProvider;

    public DeleteResourceTypeCommandHandler(IDeleteResourceTypeCommandDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task HandleAsync(DeleteResourceTypeCommand command, CancellationToken cancellationToken = default)
    {
        var resourceType = await _dataProvider.GetResourceTypeAsync(command.TypeId, cancellationToken);

        if (resourceType is null)
            throw new ResourcesDomainException("Resource type not found.");

        var group = await _dataProvider.GetGroupAsync(resourceType.GroupId, cancellationToken);

        if (group is null || group.OwnerId != command.RequesterId)
            throw new ResourcesForbiddenException("Only the group owner may delete a resource type.");

        var hasInstances = await _dataProvider.HasInstancesAsync(command.TypeId, cancellationToken);

        if (hasInstances)
            throw new ResourcesDomainException("Cannot delete a resource type that has instances.");

        _dataProvider.RemoveResourceType(resourceType);
        await _dataProvider.SaveChangesAsync(cancellationToken);
    }
}
