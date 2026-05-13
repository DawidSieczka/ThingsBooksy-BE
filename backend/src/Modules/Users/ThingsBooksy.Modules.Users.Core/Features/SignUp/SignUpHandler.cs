using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThingsBooksy.Modules.Users.Core.Entities;
using ThingsBooksy.Modules.Users.Core.Exceptions;
using ThingsBooksy.Modules.Users.Core.Features.SignUp.DataProviders;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Events.Users;
using ThingsBooksy.Shared.Abstractions.Messaging;
using ThingsBooksy.Shared.Abstractions.Time;
using ThingsBooksy.Shared.Infrastructure.Security;

namespace ThingsBooksy.Modules.Users.Core.Features.SignUp;

internal sealed class SignUpHandler : ICommandHandler<SignUpCommand>
{
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    private const string DefaultRole = Role.User;

    private readonly ISignUpCommandDataProvider _provider;
    private readonly IPasswordManager _passwordManager;
    private readonly IClock _clock;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<SignUpHandler> _logger;

    public SignUpHandler(
        ISignUpCommandDataProvider provider,
        IPasswordManager passwordManager,
        IClock clock,
        IMessageBroker messageBroker,
        ILogger<SignUpHandler> logger)
    {
        _provider = provider;
        _passwordManager = passwordManager;
        _clock = clock;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    public async Task HandleAsync(SignUpCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || !EmailAddressAttribute.IsValid(command.Email))
            throw new UsersDomainException($"Email '{command.Email}' is invalid.");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new UsersDomainException("Password cannot be empty.");

        var email = command.Email.ToLowerInvariant();

        if (await _provider.GetByEmailAsync(email, cancellationToken) is not null)
            throw new UsersDomainException("Email is already in use.");

        var roleName = string.IsNullOrWhiteSpace(command.Role) ? DefaultRole : command.Role.ToLowerInvariant();
        var role = await _provider.GetRoleAsync(roleName, cancellationToken)
            ?? throw new UsersDomainException($"Role '{roleName}' was not found.");

        var user = User.Create(command, _passwordManager.Secure(command.Password), role.Name, _clock.CurrentDate());

        try
        {
            await _provider.AddUserAsync(user, cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new UsersDomainException("Email is already in use.");
        }

        await _messageBroker.PublishAsync(new UserSignedUp(user.Id, email), cancellationToken);
        _logger.LogInformation("User with ID: '{UserId}' has signed up.", user.Id);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        => ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
        || ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true;
}
