using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember;

internal record AddGroupMemberCommand(Guid GroupId, string Email, Guid RequesterId) : ICommand;
