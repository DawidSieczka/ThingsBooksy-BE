namespace ThingsBooksy.Modules.Users.Core.Entities;

internal class Role
{
    public string Name { get; private set; } = null!;
    public IEnumerable<string> Permissions { get; private set; } = [];
    public ICollection<User> Users { get; private set; } = new List<User>();

    private Role() { }

    public static Role Create(string name, IEnumerable<string> permissions)
        => new() { Name = name, Permissions = permissions };

    public static string Default => User;

    public const string User = "user";
    public const string Admin = "admin";
}
