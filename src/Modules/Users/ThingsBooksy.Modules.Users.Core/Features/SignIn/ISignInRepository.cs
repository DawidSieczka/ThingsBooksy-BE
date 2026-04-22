using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.SignIn;

internal interface ISignInRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
