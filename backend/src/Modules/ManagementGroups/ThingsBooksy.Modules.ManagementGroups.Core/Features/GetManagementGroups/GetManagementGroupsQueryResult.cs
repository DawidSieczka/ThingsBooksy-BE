using System;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups;

internal record GetManagementGroupsQueryResult(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    DateTime CreatedAt,
    int MemberCount);
