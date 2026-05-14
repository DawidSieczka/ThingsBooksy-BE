using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstances;

internal record GetResourceInstancesQueryResult(
    IReadOnlyList<ResourceInstanceRowDto> Items,
    Guid? NextCursor);
