using System;

namespace ThingsBooksy.Modules.Resources.Core.ReadModels;

internal class GroupMemberReadModel
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
}
