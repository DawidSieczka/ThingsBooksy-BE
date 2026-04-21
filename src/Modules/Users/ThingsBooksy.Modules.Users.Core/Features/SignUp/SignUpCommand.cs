using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Users.Core.Features.SignUp;

public record SignUpCommand(Guid UserId, string Email, string Password, string? JobTitle = null, string? Role = null)
    : ICommand;
