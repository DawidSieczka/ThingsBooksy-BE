using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ThingsBooksy.Modules.Users.Contracts.Events;
using ThingsBooksy.Modules.Users.Core.Exceptions;
using ThingsBooksy.Modules.Users.Core.Services;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Messaging;
using ThingsBooksy.Shared.Infrastructure.Auth.JWT;
using ThingsBooksy.Shared.Infrastructure.Security;

namespace ThingsBooksy.Modules.Users.Core.Features.SignIn;

internal sealed class SignInHandler : ICommandHandler<SignInCommand>
{
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();

    private readonly ISignInRepository _repository;
    private readonly IJsonWebTokenManager _jwtManager;
    private readonly IPasswordManager _passwordManager;
    private readonly ITokenStorage _tokenStorage;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<SignInHandler> _logger;

    public SignInHandler(
        ISignInRepository repository,
        IJsonWebTokenManager jwtManager,
        IPasswordManager passwordManager,
        ITokenStorage tokenStorage,
        IMessageBroker messageBroker,
        ILogger<SignInHandler> logger)
    {
        _repository = repository;
        _jwtManager = jwtManager;
        _passwordManager = passwordManager;
        _tokenStorage = tokenStorage;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    public async Task HandleAsync(SignInCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || !EmailAddressAttribute.IsValid(command.Email))
            throw new UsersDomainException($"Email '{command.Email}' is invalid.");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new UsersDomainException("Password cannot be empty.");

        var user = await _repository.GetByEmailAsync(command.Email.ToLowerInvariant());
        if (user is null || !_passwordManager.IsValid(command.Password, user.Password))
            throw new UsersDomainException("Invalid credentials.");

        var claims = new Dictionary<string, IEnumerable<string>>
        {
            ["permissions"] = user.Role.Permissions
        };

        var jwt = _jwtManager.CreateToken(user.Id.ToString(), user.Email, user.Role.Name, claims: claims);
        jwt.Email = user.Email;

        await _messageBroker.PublishAsync(new SignedIn(user.Id), cancellationToken);
        _logger.LogInformation("User with ID: '{UserId}' has signed in.", user.Id);
        _tokenStorage.Set(jwt);
    }
}
