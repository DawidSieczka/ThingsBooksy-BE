using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Modules.Resources.Core.Exceptions;

public class ResourcesDomainException : CustomException
{
    public ResourcesDomainException(string message) : base(message) { }
}
