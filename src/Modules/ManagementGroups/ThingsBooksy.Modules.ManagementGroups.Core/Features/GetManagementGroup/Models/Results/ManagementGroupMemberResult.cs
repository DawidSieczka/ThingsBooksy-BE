using System;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup.Models.Results;

internal record ManagementGroupMemberResult(Guid UserId, DateTime JoinedAt);
