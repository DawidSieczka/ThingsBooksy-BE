using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RemoveGroupMember.DataProviders;

internal sealed class RemoveGroupMemberCommandDataProvider : IRemoveGroupMemberCommandDataProvider
{
    private readonly ManagementGroupsDbContext _dbContext;

    public RemoveGroupMemberCommandDataProvider(ManagementGroupsDbContext dbContext)
        => _dbContext = dbContext;

    public Task<ManagementGroup?> GetGroupWithMembersAsync(Guid groupId, CancellationToken ct)
        => _dbContext.ManagementGroups.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == groupId, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
