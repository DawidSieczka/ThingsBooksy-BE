using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.ReadModels;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Resources.Core.Features.UpdateResourceType;

internal interface IUpdateResourceTypeCommandDataProvider : IDataProvider
{
    Task<ResourceType?> GetResourceTypeWithDefinitionsAsync(Guid typeId, CancellationToken ct);
    Task<GroupReadModel?> GetGroupAsync(Guid groupId, CancellationToken ct);
    void RemovePropertyDefinitions(IEnumerable<ResourcePropertyDefinition> definitions);
    Task AddPropertyDefinitionAsync(ResourcePropertyDefinition definition, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
