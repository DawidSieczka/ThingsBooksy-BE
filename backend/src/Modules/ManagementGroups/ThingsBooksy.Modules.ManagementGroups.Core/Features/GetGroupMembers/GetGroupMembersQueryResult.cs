using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetGroupMembers;

internal sealed record GetGroupMembersQueryResult(
    IReadOnlyList<GroupMemberDto> Items,
    Guid? NextCursor);
