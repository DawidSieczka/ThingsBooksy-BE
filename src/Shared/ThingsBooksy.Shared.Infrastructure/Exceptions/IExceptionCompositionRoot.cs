using System;
using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Shared.Infrastructure.Exceptions;

public interface IExceptionCompositionRoot
{
    ExceptionResponse Map(Exception exception);
}