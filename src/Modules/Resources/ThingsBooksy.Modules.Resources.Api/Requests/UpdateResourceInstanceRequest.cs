using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.Resources.Api.Requests;

internal record UpdatePropertyValueDto(Guid PropertyDefinitionId, string Value);

internal record UpdateResourceInstanceRequest(string Name, string? Description, IEnumerable<UpdatePropertyValueDto>? PropertyValues);
