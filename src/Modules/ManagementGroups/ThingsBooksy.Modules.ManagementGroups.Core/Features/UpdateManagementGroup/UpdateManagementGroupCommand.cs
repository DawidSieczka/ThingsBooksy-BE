using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup;

public record UpdateManagementGroupCommand(Guid GroupId, string Name, string? Description, Guid RequesterId) : ICommand;
