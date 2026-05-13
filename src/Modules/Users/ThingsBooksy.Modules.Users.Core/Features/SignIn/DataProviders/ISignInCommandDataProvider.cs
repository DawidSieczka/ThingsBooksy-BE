using ThingsBooksy.Modules.Users.Core.Entities;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Users.Core.Features.SignIn.DataProviders;

internal interface ISignInCommandDataProvider : IDataProvider
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
