using System;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceType.Models;

internal record PropertyDefinitionResult(Guid Id, string Name, string DataType, bool IsRequired);
