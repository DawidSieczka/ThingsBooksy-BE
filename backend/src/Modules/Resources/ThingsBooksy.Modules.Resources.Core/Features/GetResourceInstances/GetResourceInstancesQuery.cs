using System;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstances;

internal record GetResourceInstancesQuery(
    Guid? ResourceTypeId,
    Guid? GroupId,
    bool IncludeDeleted,
    Guid RequesterId,
    Guid? AfterId,
    int Take) : IQuery<GetResourceInstancesQueryResult>;
