using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup;

internal record UpdateManagementGroupCommand(Guid GroupId, string Name, string? Description, Guid RequesterId) : ICommand;
