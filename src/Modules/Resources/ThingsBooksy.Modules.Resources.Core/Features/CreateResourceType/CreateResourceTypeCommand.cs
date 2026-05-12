using System;
using System.Collections.Generic;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Resources.Core.Features.CreateResourceType;

internal record PropertyDefinitionInput(string Name, PropertyDataType DataType, bool IsRequired);

internal record CreateResourceTypeCommand(
    Guid TypeId,
    Guid GroupId,
    Guid CallerId,
    string Name,
    string? Description,
    IEnumerable<PropertyDefinitionInput> PropertyDefinitions
) : ICommand;
