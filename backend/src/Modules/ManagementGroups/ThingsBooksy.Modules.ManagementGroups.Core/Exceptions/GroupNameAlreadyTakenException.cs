using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;

internal sealed class GroupNameAlreadyTakenException : CustomException
{
    public GroupNameAlreadyTakenException(string name)
        : base($"You already own a group with this name.")
    {
    }
}
