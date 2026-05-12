using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.Resources.Api;

public record PropertyValueInputDto(Guid PropertyDefinitionId, string Value);

public record CreateResourceInstanceRequest(
    Guid ResourceTypeId,
    string Name,
    string? Description,
    IEnumerable<PropertyValueInputDto> PropertyValues
);
