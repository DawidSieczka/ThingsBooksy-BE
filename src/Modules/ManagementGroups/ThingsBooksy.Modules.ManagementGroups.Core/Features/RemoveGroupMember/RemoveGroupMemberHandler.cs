using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RemoveGroupMember;

internal sealed class RemoveGroupMemberHandler : ICommandHandler<RemoveGroupMemberCommand>
{
    private readonly ManagementGroupsDbContext _dbContext;

    public RemoveGroupMemberHandler(ManagementGroupsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(RemoveGroupMemberCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _dbContext.ManagementGroups
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == command.GroupId, cancellationToken)
            ?? throw new ManagementGroupsDomainException($"Management group '{command.GroupId}' was not found.");

        if (group.OwnerId != command.RequesterId)
            throw new ManagementGroupsForbiddenException("Only the owner can remove members from this group.");

        if (command.UserId == group.OwnerId)
            throw new ManagementGroupsForbiddenException("The owner cannot be removed from the group.");

        var member = group.Members.FirstOrDefault(x => x.UserId == command.UserId)
            ?? throw new ManagementGroupsDomainException($"User '{command.UserId}' is not a member of this group.");

        group.Members.Remove(member);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
