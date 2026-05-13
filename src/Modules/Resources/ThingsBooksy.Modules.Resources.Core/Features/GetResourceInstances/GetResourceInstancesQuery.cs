using System;
using System.Collections.Generic;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstances;

internal record GetResourceInstancesQuery(
    Guid? ResourceTypeId,
    Guid? GroupId,
    bool IncludeDeleted,
    Guid RequesterId) : IQuery<IReadOnlyList<GetResourceInstancesQueryResult>>;
