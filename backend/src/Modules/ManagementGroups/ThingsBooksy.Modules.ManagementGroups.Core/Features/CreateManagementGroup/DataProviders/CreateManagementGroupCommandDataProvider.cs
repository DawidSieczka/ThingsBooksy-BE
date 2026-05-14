using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.CreateManagementGroup.DataProviders;

internal sealed class CreateManagementGroupCommandDataProvider : ICreateManagementGroupCommandDataProvider
{
    private readonly ManagementGroupsDbContext _dbContext;

    public CreateManagementGroupCommandDataProvider(ManagementGroupsDbContext dbContext)
        => _dbContext = dbContext;

    public Task<bool> OwnerNameExistsAsync(Guid ownerId, string name, CancellationToken ct)
        => _dbContext.ManagementGroups.AnyAsync(
            x => x.OwnerId == ownerId && EF.Functions.ILike(x.Name, name), ct);

    public Task AddAsync(ManagementGroup group, CancellationToken ct)
        => _dbContext.ManagementGroups.AddAsync(group, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
