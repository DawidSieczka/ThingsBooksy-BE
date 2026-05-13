using System;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstance.Models;

internal record PropertyValueResult(
    Guid PropertyDefinitionId,
    string PropertyName,
    string DataType,
    string Value);
