using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;

public class ManagementGroupsDomainException : CustomException
{
    public ManagementGroupsDomainException(string message) : base(message) { }
}
