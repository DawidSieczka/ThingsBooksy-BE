using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember;

internal sealed class AddGroupMemberHandler : ICommandHandler<AddGroupMemberCommand>
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    private readonly ManagementGroupsDbContext _dbContext;
    private readonly IClock _clock;

    public AddGroupMemberHandler(ManagementGroupsDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(AddGroupMemberCommand command, CancellationToken cancellationToken = default)
    {
        if (!EmailValidator.IsValid(command.Email))
            throw new ManagementGroupsDomainException($"Email '{command.Email}' has invalid format.");

        var group = await _dbContext.ManagementGroups
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can add members to this group.");

        var userReadModel = await _dbContext.UserReadModels
            .FirstOrDefaultAsync(x => x.Email == command.Email.ToLowerInvariant(), cancellationToken)
            ?? throw new ManagementGroupsDomainException($"User with email '{command.Email}' was not found.");

        if (userReadModel.Id == group.OwnerId)
            throw new ManagementGroupsDomainException("Owner is already a member of this group.");

        if (group.Members.Any(x => x.UserId == userReadModel.Id))
            throw new ManagementGroupsDomainException("User is already a member of this group.");

        var member = GroupMember.Create(group.Id, userReadModel.Id, _clock.CurrentDate());
        group.Members.Add(member);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
