using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.Resources.Api;

public record UpdatePropertyValueDto(Guid PropertyDefinitionId, string Value);

public record UpdateResourceInstanceRequest(string Name, string? Description, IEnumerable<UpdatePropertyValueDto>? PropertyValues);
