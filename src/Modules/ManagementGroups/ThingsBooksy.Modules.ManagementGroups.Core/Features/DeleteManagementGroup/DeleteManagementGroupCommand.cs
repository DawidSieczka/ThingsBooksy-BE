using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup;

public record DeleteManagementGroupCommand(Guid GroupId, Guid RequesterId) : ICommand;
