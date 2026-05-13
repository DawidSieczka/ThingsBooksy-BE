using ThingsBooksy.Modules.Users.Core.Entities;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Users.Core.Features.SignUp.DataProviders;

internal interface ISignUpCommandDataProvider : IDataProvider
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
}
