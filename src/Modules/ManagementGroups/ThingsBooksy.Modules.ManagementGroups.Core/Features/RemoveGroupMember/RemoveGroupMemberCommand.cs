using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RemoveGroupMember;

internal record RemoveGroupMemberCommand(Guid GroupId, Guid UserId, Guid RequesterId) : ICommand;
