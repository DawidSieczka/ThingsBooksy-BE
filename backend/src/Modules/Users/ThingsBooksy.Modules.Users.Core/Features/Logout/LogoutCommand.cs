using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Users.Core.Features.Logout;

internal record LogoutCommand(string Jti, Guid UserId, DateTime ExpiresAt) : ICommand;
