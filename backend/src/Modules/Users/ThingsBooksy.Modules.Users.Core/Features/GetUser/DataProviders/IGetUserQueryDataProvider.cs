using ThingsBooksy.Modules.Users.Core.Entities;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Users.Core.Features.GetUser.DataProviders;

internal interface IGetUserQueryDataProvider : IDataProvider
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
