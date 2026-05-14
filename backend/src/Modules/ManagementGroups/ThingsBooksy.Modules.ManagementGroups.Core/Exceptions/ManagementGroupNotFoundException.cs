using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;

internal sealed class ManagementGroupNotFoundException : NotFoundException
{
    public ManagementGroupNotFoundException(string message) : base(message)
    {
    }
}
