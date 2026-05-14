using System;
using System.Collections.Generic;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup.Models.Results;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup;

internal record GetManagementGroupQueryResult(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    DateTimeOffset CreatedAt,
    int MemberCount,
    IReadOnlyList<ManagementGroupMemberResult> Members);
