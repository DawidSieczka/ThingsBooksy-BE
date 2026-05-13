using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember.DataProviders;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;
using ThingsBooksy.Shared.Abstractions.Messaging;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember;

internal sealed class AddGroupMemberCommandHandler : ICommandHandler<AddGroupMemberCommand>
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    private readonly IAddGroupMemberCommandDataProvider _provider;
    private readonly IClock _clock;
    private readonly IMessageBroker _messageBroker;

    public AddGroupMemberCommandHandler(
        IAddGroupMemberCommandDataProvider provider,
        IClock clock,
        IMessageBroker messageBroker)
    {
        _provider = provider;
        _clock = clock;
        _messageBroker = messageBroker;
    }

    public async Task HandleAsync(AddGroupMemberCommand command, CancellationToken cancellationToken = default)
    {
        if (!EmailValidator.IsValid(command.Email))
            throw new ManagementGroupsDomainException($"Email '{command.Email}' has invalid format.");

        var group = await _provider.GetGroupWithMembersAsync(command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can add members to this group.");

        var userReadModel = await _provider.GetUserByEmailAsync(command.Email.ToLowerInvariant(), cancellationToken)
            ?? throw new UserNotFoundException(command.Email);

        if (userReadModel.Id == group.OwnerId)
            throw new ManagementGroupsDomainException("Owner is already a member of this group.");

        if (group.Members.Any(x => x.UserId == userReadModel.Id))
            throw new ManagementGroupsDomainException("User is already a member of this group.");

        var member = GroupMember.Create(command, userReadModel.Id, _clock.CurrentDate());
        group.Members.Add(member);
        await _provider.SaveChangesAsync(cancellationToken);
        await _messageBroker.PublishAsync(new GroupMemberAdded(command.GroupId, userReadModel.Id), cancellationToken);
    }
}
