using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.GetUser;

internal interface IGetUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
