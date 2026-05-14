using System;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetGroupMembers;

internal sealed record GroupMemberDto(
    Guid MemberId,
    Guid UserId,
    string Email,
    DateTimeOffset JoinedAt,
    bool IsOwner);
