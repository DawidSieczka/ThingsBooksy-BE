using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ThingsBooksy.Modules.Users.Core.Exceptions;
using ThingsBooksy.Modules.Users.Core.Features.SignIn.DataProviders;
using ThingsBooksy.Modules.Users.Core.Services;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Infrastructure.Auth.JWT;
using ThingsBooksy.Shared.Infrastructure.Security;

namespace ThingsBooksy.Modules.Users.Core.Features.SignIn;

internal sealed class SignInHandler : ICommandHandler<SignInCommand>
{
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();

    private readonly ISignInCommandDataProvider _provider;
    private readonly IJsonWebTokenManager _jwtManager;
    private readonly IPasswordManager _passwordManager;
    private readonly ITokenStorage _tokenStorage;
    private readonly ILogger<SignInHandler> _logger;

    public SignInHandler(
        ISignInCommandDataProvider provider,
        IJsonWebTokenManager jwtManager,
        IPasswordManager passwordManager,
        ITokenStorage tokenStorage,
        ILogger<SignInHandler> logger)
    {
        _provider = provider;
        _jwtManager = jwtManager;
        _passwordManager = passwordManager;
        _tokenStorage = tokenStorage;
        _logger = logger;
    }

    public async Task HandleAsync(SignInCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || !EmailAddressAttribute.IsValid(command.Email))
            throw new UsersDomainException($"Email '{command.Email}' is invalid.");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new UsersDomainException("Password cannot be empty.");

        var user = await _provider.GetByEmailAsync(command.Email.ToLowerInvariant(), cancellationToken);

        // Always run password verification to prevent timing-based email enumeration.
        var storedHash = user?.Password ?? "$2a$11$dummy.hash.to.prevent.timing.attack.padding.x";
        if (user is null || !_passwordManager.IsValid(command.Password, storedHash))
            throw new UsersDomainException("Invalid credentials.");

        var claims = new Dictionary<string, IEnumerable<string>>
        {
            ["permissions"] = user.Role.Permissions
        };

        var jwt = _jwtManager.CreateToken(user.Id.ToString(), user.Email, user.Role.Name, claims: claims);
        jwt.Email = user.Email;

        _logger.LogInformation("User with ID: '{UserId}' has signed in.", user.Id);
        _tokenStorage.Set(jwt);
    }
}
