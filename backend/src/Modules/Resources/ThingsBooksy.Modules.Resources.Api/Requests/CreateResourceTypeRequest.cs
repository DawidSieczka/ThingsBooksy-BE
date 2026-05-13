using System;
using System.Collections.Generic;
using ThingsBooksy.Modules.Resources.Core.Domain;

namespace ThingsBooksy.Modules.Resources.Api.Requests;

internal record PropertyDefinitionInputDto(string Name, PropertyDataType DataType, bool IsRequired);

internal record CreateResourceTypeRequest(
    Guid GroupId,
    string Name,
    string? Description,
    IEnumerable<PropertyDefinitionInputDto>? PropertyDefinitions
);
