using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RestoreManagementGroup.DataProviders;

internal sealed class RestoreManagementGroupCommandDataProvider : IRestoreManagementGroupCommandDataProvider
{
    private readonly ManagementGroupsDbContext _dbContext;

    public RestoreManagementGroupCommandDataProvider(ManagementGroupsDbContext dbContext)
        => _dbContext = dbContext;

    public Task<ManagementGroup?> GetByIdIgnoringFiltersAsync(Guid groupId, CancellationToken ct)
        => _dbContext.ManagementGroups.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == groupId, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
