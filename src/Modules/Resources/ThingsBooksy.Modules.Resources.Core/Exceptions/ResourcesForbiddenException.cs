using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Modules.Resources.Core.Exceptions;

public class ResourcesForbiddenException : ForbiddenException
{
    public ResourcesForbiddenException(string message) : base(message) { }
}
