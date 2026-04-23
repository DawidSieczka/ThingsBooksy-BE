using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RestoreManagementGroup;

public record RestoreManagementGroupCommand(Guid GroupId, Guid RequesterId) : ICommand;
