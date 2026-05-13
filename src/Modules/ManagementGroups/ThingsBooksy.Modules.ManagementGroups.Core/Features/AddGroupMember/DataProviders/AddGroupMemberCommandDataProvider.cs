using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember.DataProviders;

internal sealed class AddGroupMemberCommandDataProvider : IAddGroupMemberCommandDataProvider
{
    private readonly ManagementGroupsDbContext _dbContext;

    public AddGroupMemberCommandDataProvider(ManagementGroupsDbContext dbContext)
        => _dbContext = dbContext;

    public Task<ManagementGroup?> GetGroupWithMembersAsync(Guid groupId, CancellationToken ct)
        => _dbContext.ManagementGroups.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == groupId, ct);

    public Task<UserReadModel?> GetUserByEmailAsync(string email, CancellationToken ct)
        => _dbContext.UserReadModels.FirstOrDefaultAsync(x => x.Email == email, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
