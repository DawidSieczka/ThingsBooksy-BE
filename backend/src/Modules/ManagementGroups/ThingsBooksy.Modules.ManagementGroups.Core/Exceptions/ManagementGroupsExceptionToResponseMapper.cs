using System;
using System.Net;
using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;

internal sealed class ManagementGroupsExceptionToResponseMapper : IExceptionToResponseMapper
{
    public ExceptionResponse Map(Exception exception)
        => exception switch
        {
            GroupNameAlreadyTakenException ex => new ExceptionResponse(
                new { code = "GROUP_NAME_TAKEN", message = ex.Message },
                HttpStatusCode.Conflict),
            _ => null!
        };
}
