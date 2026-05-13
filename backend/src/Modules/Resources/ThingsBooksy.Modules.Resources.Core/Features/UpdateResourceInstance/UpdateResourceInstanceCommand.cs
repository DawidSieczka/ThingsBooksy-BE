using System;
using System.Collections.Generic;
using ThingsBooksy.Modules.Resources.Core.Features.CreateResourceInstance;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Resources.Core.Features.UpdateResourceInstance;

internal record UpdateResourceInstanceCommand(
    Guid InstanceId,
    string Name,
    string? Description,
    IEnumerable<PropertyValueInput> PropertyValues,
    Guid RequesterId
) : ICommand;
