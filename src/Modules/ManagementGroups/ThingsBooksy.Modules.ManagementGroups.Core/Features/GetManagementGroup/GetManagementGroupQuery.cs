using System;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup;

internal record GetManagementGroupQuery(Guid GroupId, Guid RequesterId) : IQuery<GetManagementGroupQueryResult?>;
