using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.ReadModels;

namespace ThingsBooksy.Modules.Resources.Core.Features.CreateResourceType;

internal sealed class CreateResourceTypeCommandDataProvider : ICreateResourceTypeCommandDataProvider
{
    private readonly ResourcesDbContext _dbContext;

    public CreateResourceTypeCommandDataProvider(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public Task<GroupReadModel?> GetGroupAsync(Guid groupId, CancellationToken ct)
        => _dbContext.GroupReadModels.FirstOrDefaultAsync(g => g.Id == groupId, ct);

    public Task<bool> ExistsByGroupAndNameAsync(Guid groupId, string normalizedName, Guid? excludeId, CancellationToken ct)
        => _dbContext.ResourceTypes.IgnoreQueryFilters().AnyAsync(
            t => t.GroupId == groupId && t.Name == normalizedName && (excludeId == null || t.Id != excludeId.Value) && t.DeletedAt == null,
            ct);

    public Task AddResourceTypeAsync(ResourceType resourceType, CancellationToken ct)
        => _dbContext.ResourceTypes.AddAsync(resourceType, ct).AsTask();

    public Task AddPropertyDefinitionAsync(ResourcePropertyDefinition definition, CancellationToken ct)
        => _dbContext.ResourcePropertyDefinitions.AddAsync(definition, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);
}
