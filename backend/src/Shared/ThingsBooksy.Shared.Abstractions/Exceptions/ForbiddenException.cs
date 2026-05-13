namespace ThingsBooksy.Shared.Abstractions.Exceptions;

public abstract class ForbiddenException : CustomException
{
    protected ForbiddenException(string message) : base(message)
    {
    }
}
