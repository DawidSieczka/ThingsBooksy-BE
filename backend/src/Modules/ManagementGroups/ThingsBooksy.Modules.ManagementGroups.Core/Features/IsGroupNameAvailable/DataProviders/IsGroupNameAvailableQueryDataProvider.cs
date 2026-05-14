using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.IsGroupNameAvailable.DataProviders;

internal sealed class IsGroupNameAvailableQueryDataProvider : IIsGroupNameAvailableQueryDataProvider
{
    private readonly ManagementGroupsDbContext _db;

    public IsGroupNameAvailableQueryDataProvider(ManagementGroupsDbContext db)
        => _db = db;

    public Task<bool> ExistsAsync(Guid ownerId, string name, CancellationToken ct)
        => _db.ManagementGroups.AnyAsync(
            g => g.OwnerId == ownerId && EF.Functions.ILike(g.Name, name), ct);
}
