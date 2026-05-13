using System;
using System.Collections.Generic;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups;

internal record GetManagementGroupsQuery(Guid UserId) : IQuery<IEnumerable<GetManagementGroupsQueryResult>>;
