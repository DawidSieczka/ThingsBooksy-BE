using System;

namespace ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;

internal class UserReadModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
}
