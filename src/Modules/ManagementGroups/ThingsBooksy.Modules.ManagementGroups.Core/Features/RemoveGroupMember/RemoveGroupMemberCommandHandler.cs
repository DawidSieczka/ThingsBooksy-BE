using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.RemoveGroupMember.DataProviders;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;
using ThingsBooksy.Shared.Abstractions.Messaging;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RemoveGroupMember;

internal sealed class RemoveGroupMemberCommandHandler : ICommandHandler<RemoveGroupMemberCommand>
{
    private readonly IRemoveGroupMemberCommandDataProvider _provider;
    private readonly IMessageBroker _messageBroker;

    public RemoveGroupMemberCommandHandler(IRemoveGroupMemberCommandDataProvider provider, IMessageBroker messageBroker)
    {
        _provider = provider;
        _messageBroker = messageBroker;
    }

    public async Task HandleAsync(RemoveGroupMemberCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _provider.GetGroupWithMembersAsync(command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can remove members from this group.");

        if (command.UserId == group.OwnerId)
            throw new ManagementGroupsForbiddenException("The owner cannot be removed from the group.");

        var member = group.Members.FirstOrDefault(x => x.UserId == command.UserId)
            ?? throw new ManagementGroupsDomainException($"User '{command.UserId}' is not a member of this group.");

        group.Members.Remove(member);
        await _provider.SaveChangesAsync(cancellationToken);
        await _messageBroker.PublishAsync(new GroupMemberRemoved(command.GroupId, command.UserId), cancellationToken);
    }
}
