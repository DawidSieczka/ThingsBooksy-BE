using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.CreateManagementGroup;

public record CreateManagementGroupCommand(Guid GroupId, string Name, string? Description, Guid OwnerId) : ICommand;
