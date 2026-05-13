using ThingsBooksy.Modules.Users.Core.Features.SignUp;

namespace ThingsBooksy.Modules.Users.Core.Entities;

internal class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public string? JobTitle { get; private set; }
    public string RoleName { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private User() { }

    private const string DefaultJobTitle = "member";

    public static User Create(SignUpCommand command, string hashedPassword, string roleName, DateTime now)
        => new()
        {
            Id = Guid.CreateVersion7(),
            Email = command.Email.ToLowerInvariant(),
            Password = hashedPassword,
            JobTitle = string.IsNullOrWhiteSpace(command.JobTitle)
                ? DefaultJobTitle
                : command.JobTitle.ToLowerInvariant(),
            RoleName = roleName,
            CreatedAt = now
        };

    public Role Role { get; private set; } = null!;
}
