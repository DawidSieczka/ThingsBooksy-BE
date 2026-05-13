using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup;

internal record DeleteManagementGroupCommand(Guid GroupId, Guid RequesterId) : ICommand;
