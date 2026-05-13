using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.ReadModels;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Resources.Core.Features.CreateResourceInstance;

internal interface ICreateResourceInstanceCommandDataProvider : IDataProvider
{
    Task<ResourceType?> GetResourceTypeAsync(Guid resourceTypeId, CancellationToken ct);
    Task<GroupReadModel?> GetGroupAsync(Guid groupId, CancellationToken ct);
    Task<bool> NameExistsAsync(Guid resourceTypeId, string name, CancellationToken ct);
    Task<List<ResourcePropertyDefinition>> GetPropertyDefinitionsAsync(Guid resourceTypeId, CancellationToken ct);
    Task AddResourceInstanceAsync(ResourceInstance instance, CancellationToken ct);
    Task AddPropertyValueAsync(ResourcePropertyValue propertyValue, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
