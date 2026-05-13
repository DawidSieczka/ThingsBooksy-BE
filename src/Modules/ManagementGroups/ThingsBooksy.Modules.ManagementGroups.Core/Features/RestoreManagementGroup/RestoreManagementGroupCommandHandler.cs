using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.RestoreManagementGroup.DataProviders;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RestoreManagementGroup;

internal sealed class RestoreManagementGroupCommandHandler : ICommandHandler<RestoreManagementGroupCommand>
{
    private readonly IRestoreManagementGroupCommandDataProvider _provider;
    private readonly IClock _clock;

    public RestoreManagementGroupCommandHandler(IRestoreManagementGroupCommandDataProvider provider, IClock clock)
    {
        _provider = provider;
        _clock = clock;
    }

    public async Task HandleAsync(RestoreManagementGroupCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _provider.GetByIdIgnoringFiltersAsync(command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can restore this group.");

        if (!group.IsDeleted)
            throw new ManagementGroupsDomainException("Group is not deleted and cannot be restored.");

        group.Restore(_clock.CurrentDate());
        await _provider.SaveChangesAsync(cancellationToken);
    }
}
