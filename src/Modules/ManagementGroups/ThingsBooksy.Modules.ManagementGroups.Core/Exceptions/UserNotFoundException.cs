using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;

internal sealed class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(string email) : base($"User with email '{email}' was not found.") { }
}
