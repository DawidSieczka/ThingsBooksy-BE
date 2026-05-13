using System;
using System.Collections.Generic;
using ThingsBooksy.Modules.Resources.Core.Features.GetResourceType.Models;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceType;

internal record GetResourceTypeQueryResult(
    Guid Id,
    Guid GroupId,
    string Name,
    string? Description,
    DateTime CreatedAt,
    IReadOnlyList<PropertyDefinitionResult> PropertyDefinitions);
