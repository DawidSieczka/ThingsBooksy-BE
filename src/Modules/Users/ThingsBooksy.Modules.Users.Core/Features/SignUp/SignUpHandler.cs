using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ThingsBooksy.Modules.Users.Contracts.Events;
using ThingsBooksy.Modules.Users.Core.Entities;
using ThingsBooksy.Modules.Users.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Messaging;
using ThingsBooksy.Shared.Abstractions.Time;
using ThingsBooksy.Shared.Infrastructure.Security;

namespace ThingsBooksy.Modules.Users.Core.Features.SignUp;

internal sealed class SignUpHandler : ICommandHandler<SignUpCommand>
{
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    private const string DefaultRole = Role.User;
    private const string DefaultJobTitle = "member";

    private readonly ISignUpRepository _repository;
    private readonly IPasswordManager _passwordManager;
    private readonly IClock _clock;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<SignUpHandler> _logger;

    public SignUpHandler(
        ISignUpRepository repository,
        IPasswordManager passwordManager,
        IClock clock,
        IMessageBroker messageBroker,
        ILogger<SignUpHandler> logger)
    {
        _repository = repository;
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

        if (await _repository.GetByEmailAsync(email) is not null)
            throw new UsersDomainException("Email is already in use.");

        var roleName = string.IsNullOrWhiteSpace(command.Role) ? DefaultRole : command.Role.ToLowerInvariant();
        var role = await _repository.GetRoleAsync(roleName)
            ?? throw new UsersDomainException($"Role '{roleName}' was not found.");

        var jobTitle = string.IsNullOrWhiteSpace(command.JobTitle)
            ? DefaultJobTitle
            : command.JobTitle.ToLowerInvariant();

        var user = new User
        {
            Id = command.UserId,
            Email = email,
            Password = _passwordManager.Secure(command.Password),
            Role = role,
            JobTitle = jobTitle,
            CreatedAt = _clock.CurrentDate()
        };

        await _repository.AddUserAsync(user);
        await _messageBroker.PublishAsync(new SignedUp(user.Id, email, role.Name, jobTitle), cancellationToken);
        _logger.LogInformation("User with ID: '{UserId}' has signed up.", user.Id);
    }
}
