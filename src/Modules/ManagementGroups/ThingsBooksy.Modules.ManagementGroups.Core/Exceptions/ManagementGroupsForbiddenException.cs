using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;

public class ManagementGroupsForbiddenException : ForbiddenException
{
    public ManagementGroupsForbiddenException(string message) : base(message) { }
}
