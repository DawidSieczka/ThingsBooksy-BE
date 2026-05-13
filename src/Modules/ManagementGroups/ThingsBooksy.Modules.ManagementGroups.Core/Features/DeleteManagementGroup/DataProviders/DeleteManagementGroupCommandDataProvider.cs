using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup.DataProviders;

internal sealed class DeleteManagementGroupCommandDataProvider : IDeleteManagementGroupCommandDataProvider
{
    private readonly ManagementGroupsDbContext _dbContext;

    public DeleteManagementGroupCommandDataProvider(ManagementGroupsDbContext dbContext)
        => _dbContext = dbContext;

    public Task<ManagementGroup?> GetByIdAsync(Guid groupId, CancellationToken ct)
        => _dbContext.ManagementGroups.FirstOrDefaultAsync(x => x.Id == groupId, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
