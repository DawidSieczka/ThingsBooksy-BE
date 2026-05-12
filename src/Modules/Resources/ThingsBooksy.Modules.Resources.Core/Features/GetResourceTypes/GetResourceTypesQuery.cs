using System;
using System.Collections.Generic;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceTypes;

internal record GetResourceTypesQuery(Guid GroupId, Guid RequesterId) : IQuery<IReadOnlyList<ResourceTypeDto>>;
