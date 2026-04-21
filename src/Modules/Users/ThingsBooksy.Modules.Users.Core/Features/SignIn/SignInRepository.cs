using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.SignIn;

internal sealed class SignInRepository : ISignInRepository
{
    private readonly UsersDbContext _dbContext;

    public SignInRepository(UsersDbContext dbContext)
        => _dbContext = dbContext;

    public Task<User?> GetByEmailAsync(string email)
        => _dbContext.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email == email);
}
