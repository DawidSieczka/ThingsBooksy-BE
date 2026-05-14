using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceType;

internal sealed class DeleteResourceTypeCommandHandler : ICommandHandler<DeleteResourceTypeCommand>
{
    private readonly IDeleteResourceTypeCommandDataProvider _dataProvider;
    private readonly IClock _clock;

    public DeleteResourceTypeCommandHandler(IDeleteResourceTypeCommandDataProvider dataProvider, IClock clock)
    {
        _dataProvider = dataProvider;
        _clock = clock;
    }

    public async Task HandleAsync(DeleteResourceTypeCommand command, CancellationToken cancellationToken = default)
    {
        var resourceType = await _dataProvider.GetResourceTypeAsync(command.TypeId, cancellationToken);

        if (resourceType is null)
            throw new ResourcesDomainException("Resource type not found.");

        var group = await _dataProvider.GetGroupAsync(resourceType.GroupId, cancellationToken);

        if (group is null || group.OwnerId != command.RequesterId)
            throw new ResourcesForbiddenException("Only the group owner may delete a resource type.");

        // Cascade soft-delete all instances of this type before hard-deleting the type.
        // ExecuteUpdateAsync is a bulk operation; zero rows affected is a no-op (idempotent).
        await _dataProvider.SoftDeleteInstancesAsync(command.TypeId, _clock.CurrentDate(), cancellationToken);

        // Hard-delete the resource type. EF cascade removes ResourcePropertyDefinitions.
        _dataProvider.RemoveResourceType(resourceType);
        await _dataProvider.SaveChangesAsync(cancellationToken);
    }
}
