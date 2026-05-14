using System;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetGroupMembers;

internal sealed record GetGroupMembersQuery(
    Guid CallerUserId,
    Guid GroupId,
    Guid? AfterId,
    int Take) : IQuery<GetGroupMembersQueryResult>;
