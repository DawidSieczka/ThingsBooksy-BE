using System.ComponentModel.DataAnnotations;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Users.Core.Features.SignIn;

internal record SignInCommand([Required][EmailAddress] string Email, [Required] string Password) : ICommand
{
    public System.Guid Id { get; init; } = System.Guid.NewGuid();
}
