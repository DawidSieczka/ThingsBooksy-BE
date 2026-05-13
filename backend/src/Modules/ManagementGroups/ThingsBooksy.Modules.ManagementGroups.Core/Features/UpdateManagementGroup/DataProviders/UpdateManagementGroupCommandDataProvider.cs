using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup.DataProviders;

internal sealed class UpdateManagementGroupCommandDataProvider : IUpdateManagementGroupCommandDataProvider
{
    private readonly ManagementGroupsDbContext _dbContext;

    public UpdateManagementGroupCommandDataProvider(ManagementGroupsDbContext dbContext)
        => _dbContext = dbContext;

    public Task<ManagementGroup?> GetByIdAsync(Guid groupId, CancellationToken ct)
        => _dbContext.ManagementGroups.FirstOrDefaultAsync(x => x.Id == groupId, ct);

    public Task<bool> NameExistsForOtherGroupAsync(string name, Guid excludedGroupId, CancellationToken ct)
        => _dbContext.ManagementGroups.IgnoreQueryFilters().AnyAsync(x => x.Name == name && x.Id != excludedGroupId, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
