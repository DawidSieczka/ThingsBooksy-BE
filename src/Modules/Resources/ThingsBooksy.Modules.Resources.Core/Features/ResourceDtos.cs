using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.Resources.Core.Features;

internal record PropertyDefinitionDto(Guid Id, string Name, string DataType, bool IsRequired);

internal record ResourceTypeDto(
    Guid Id,
    Guid GroupId,
    string Name,
    string? Description,
    DateTime CreatedAt,
    IReadOnlyList<PropertyDefinitionDto> PropertyDefinitions);

internal record PropertyValueDto(
    Guid PropertyDefinitionId,
    string PropertyName,
    string DataType,
    string Value);

internal record ResourceInstanceDto(
    Guid Id,
    Guid ResourceTypeId,
    Guid GroupId,
    string Name,
    string? Description,
    Guid OwnerId,
    DateTime CreatedAt,
    DateTime? DeletedAt,
    IReadOnlyList<PropertyValueDto> PropertyValues);
