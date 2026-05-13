using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Users.Core.Features.SignUp;

internal record SignUpCommand(string Email, string Password, string? JobTitle = null, string? Role = null) : ICommand;
