using System;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceType;

internal record GetResourceTypeQuery(Guid TypeId, Guid RequesterId) : IQuery<ResourceTypeDto?>;
