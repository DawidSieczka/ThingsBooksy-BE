using System;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstance;

internal record GetResourceInstanceQuery(Guid InstanceId, Guid RequesterId) : IQuery<ResourceInstanceDto?>;
