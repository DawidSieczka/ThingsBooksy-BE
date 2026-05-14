using ThingsBooksy.Modules.Users.Core.Features.Logout.DataProviders;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.Users.Core.Features.Logout;

internal sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly ILogoutCommandDataProvider _provider;
    private readonly IClock _clock;

    public LogoutCommandHandler(ILogoutCommandDataProvider provider, IClock clock)
    {
        _provider = provider;
        _clock = clock;
    }

    public Task HandleAsync(LogoutCommand command, CancellationToken cancellationToken = default)
        => _provider.RevokeAsync(command.Jti, command.UserId, command.ExpiresAt, _clock.CurrentDate(), cancellationToken);
}
