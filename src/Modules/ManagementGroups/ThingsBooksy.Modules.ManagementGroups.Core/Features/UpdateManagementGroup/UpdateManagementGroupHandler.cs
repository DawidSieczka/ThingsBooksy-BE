using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup;

internal sealed class UpdateManagementGroupHandler : ICommandHandler<UpdateManagementGroupCommand>
{
    private readonly ManagementGroupsDbContext _dbContext;
    private readonly IClock _clock;

    public UpdateManagementGroupHandler(ManagementGroupsDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(UpdateManagementGroupCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _dbContext.ManagementGroups.FirstOrDefaultAsync(x => x.Id == command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can update this group.");

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ManagementGroupsDomainException("Group name cannot be empty.");

        var nameExists = await _dbContext.ManagementGroups
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Name == command.Name && x.Id != command.GroupId, cancellationToken);

        if (nameExists)
            throw new ManagementGroupsDomainException($"Group name '{command.Name}' is already taken.");

        group.Update(command.Name, command.Description, _clock.CurrentDate());
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
