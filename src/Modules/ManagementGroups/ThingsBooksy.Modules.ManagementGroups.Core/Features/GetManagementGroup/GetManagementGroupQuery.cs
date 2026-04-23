using System;
using System.Collections.Generic;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup;

public record GetManagementGroupQuery(Guid GroupId, Guid RequesterId) : IQuery<ManagementGroupDetailDto?>;

public record ManagementGroupDetailDto(Guid Id, string Name, string? Description, Guid OwnerId, DateTime CreatedAt, IEnumerable<GroupMemberDto> Members);

public record GroupMemberDto(Guid UserId, DateTime JoinedAt);
