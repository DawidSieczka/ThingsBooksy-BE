using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup.DataProviders;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;
using ThingsBooksy.Shared.Abstractions.Messaging;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup;

internal sealed class DeleteManagementGroupCommandHandler : ICommandHandler<DeleteManagementGroupCommand>
{
    private readonly IDeleteManagementGroupCommandDataProvider _provider;
    private readonly IClock _clock;
    private readonly IMessageBroker _messageBroker;

    public DeleteManagementGroupCommandHandler(
        IDeleteManagementGroupCommandDataProvider provider,
        IClock clock,
        IMessageBroker messageBroker)
    {
        _provider = provider;
        _clock = clock;
        _messageBroker = messageBroker;
    }

    public async Task HandleAsync(DeleteManagementGroupCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _provider.GetByIdAsync(command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can delete this group.");

        group.Delete(_clock.CurrentDate());
        await _provider.SaveChangesAsync(cancellationToken);
        await _messageBroker.PublishAsync(new GroupDeleted(command.GroupId), cancellationToken);
    }
}
