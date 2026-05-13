using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.GetUser.DataProviders;

internal sealed class GetUserQueryDataProvider : IGetUserQueryDataProvider
{
    private readonly UsersDbContext _dbContext;

    public GetUserQueryDataProvider(UsersDbContext dbContext)
        => _dbContext = dbContext;

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
}
