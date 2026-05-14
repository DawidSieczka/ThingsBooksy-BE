using System;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.IsGroupNameAvailable;

internal sealed record IsGroupNameAvailableQuery(Guid CallerUserId, string Name) : IQuery<IsGroupNameAvailableQueryResult>;
