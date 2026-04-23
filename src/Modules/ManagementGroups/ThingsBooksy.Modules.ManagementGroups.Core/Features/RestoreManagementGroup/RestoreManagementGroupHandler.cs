using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RestoreManagementGroup;

internal sealed class RestoreManagementGroupHandler : ICommandHandler<RestoreManagementGroupCommand>
{
    private readonly ManagementGroupsDbContext _dbContext;
    private readonly IClock _clock;

    public RestoreManagementGroupHandler(ManagementGroupsDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(RestoreManagementGroupCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _dbContext.ManagementGroups
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can restore this group.");

        if (!group.IsDeleted)
            throw new ManagementGroupsDomainException("Group is not deleted and cannot be restored.");

        group.Restore(_clock.CurrentDate());
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
