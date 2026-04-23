using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup;

internal sealed class DeleteManagementGroupHandler : ICommandHandler<DeleteManagementGroupCommand>
{
    private readonly ManagementGroupsDbContext _dbContext;
    private readonly IClock _clock;

    public DeleteManagementGroupHandler(ManagementGroupsDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(DeleteManagementGroupCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _dbContext.ManagementGroups.FirstOrDefaultAsync(x => x.Id == command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can delete this group.");

        group.Delete(_clock.CurrentDate());
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
