using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.CreateManagementGroup;

internal sealed class CreateManagementGroupHandler : ICommandHandler<CreateManagementGroupCommand>
{
    private readonly ManagementGroupsDbContext _dbContext;
    private readonly IClock _clock;

    public CreateManagementGroupHandler(ManagementGroupsDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(CreateManagementGroupCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ManagementGroupsDomainException("Group name cannot be empty.");

        var nameExists = await _dbContext.ManagementGroups
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Name == command.Name, cancellationToken);

        if (nameExists)
            throw new ManagementGroupsDomainException($"Group name '{command.Name}' is already taken.");

        var group = ManagementGroup.Create(command.GroupId, command.Name, command.Description, command.OwnerId, _clock.CurrentDate());
        await _dbContext.ManagementGroups.AddAsync(group, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
