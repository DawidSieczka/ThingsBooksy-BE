using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.Users.Core.Features.GetUser;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserDetailsDto : UserDto
{
    public IEnumerable<string> Permissions { get; set; } = [];
}
