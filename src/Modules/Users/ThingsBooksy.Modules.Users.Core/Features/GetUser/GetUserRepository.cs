using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.GetUser;

internal sealed class GetUserRepository : IGetUserRepository
{
    private readonly UsersDbContext _dbContext;

    public GetUserRepository(UsersDbContext dbContext)
        => _dbContext = dbContext;

    public Task<User?> GetByIdAsync(Guid userId)
        => _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Id == userId);
}
