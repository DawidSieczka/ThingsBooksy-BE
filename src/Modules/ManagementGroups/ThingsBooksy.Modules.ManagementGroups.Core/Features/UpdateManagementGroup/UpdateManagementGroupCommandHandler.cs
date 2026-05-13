using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup.DataProviders;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup;

internal sealed class UpdateManagementGroupCommandHandler : ICommandHandler<UpdateManagementGroupCommand>
{
    private readonly IUpdateManagementGroupCommandDataProvider _provider;
    private readonly IClock _clock;

    public UpdateManagementGroupCommandHandler(IUpdateManagementGroupCommandDataProvider provider, IClock clock)
    {
        _provider = provider;
        _clock = clock;
    }

    public async Task HandleAsync(UpdateManagementGroupCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _provider.GetByIdAsync(command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can update this group.");

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ManagementGroupsDomainException("Group name cannot be empty.");

        if (await _provider.NameExistsForOtherGroupAsync(command.Name, command.GroupId, cancellationToken))
            throw new ManagementGroupsDomainException($"Group name '{command.Name}' is already taken.");

        group.Update(command, _clock.CurrentDate());
        await _provider.SaveChangesAsync(cancellationToken);
    }
}
