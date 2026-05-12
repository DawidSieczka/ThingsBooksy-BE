using System;
using System.Collections.Generic;
using ThingsBooksy.Modules.Resources.Core.Domain;

namespace ThingsBooksy.Modules.Resources.Api;

public record PropertyDefinitionUpdateInputDto(Guid? Id, string Name, PropertyDataType DataType, bool IsRequired);

public record UpdateResourceTypeRequest(string Name, string? Description, IEnumerable<PropertyDefinitionUpdateInputDto>? PropertyDefinitions);
