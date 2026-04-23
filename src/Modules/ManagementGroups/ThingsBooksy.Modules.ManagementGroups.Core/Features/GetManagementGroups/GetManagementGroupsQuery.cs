using System;
using System.Collections.Generic;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups;

public record GetManagementGroupsQuery(Guid UserId) : IQuery<IEnumerable<ManagementGroupDto>>;

public record ManagementGroupDto(Guid Id, string Name, string? Description, Guid OwnerId, DateTime CreatedAt);
