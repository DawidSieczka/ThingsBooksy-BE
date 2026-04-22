using System;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.SignUp;

internal interface ISignUpRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
}
