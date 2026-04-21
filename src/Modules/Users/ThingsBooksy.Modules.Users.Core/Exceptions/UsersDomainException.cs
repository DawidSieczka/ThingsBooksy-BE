using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Modules.Users.Core.Exceptions;

public class UsersDomainException : CustomException
{
    public UsersDomainException(string message) : base(message)
    {
    }
}
