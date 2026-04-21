using System;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.SignUp;

internal interface ISignUpRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<Role?> GetRoleAsync(string roleName);
    Task AddUserAsync(User user);
}
