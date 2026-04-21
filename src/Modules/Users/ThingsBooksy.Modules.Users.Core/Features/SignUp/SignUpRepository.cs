using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.SignUp;

internal sealed class SignUpRepository : ISignUpRepository
{
    private readonly UsersDbContext _dbContext;

    public SignUpRepository(UsersDbContext dbContext)
        => _dbContext = dbContext;

    public Task<User?> GetByEmailAsync(string email)
        => _dbContext.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email == email);

    public Task<Role?> GetRoleAsync(string roleName)
        => _dbContext.Roles.SingleOrDefaultAsync(r => r.Name == roleName);

    public async Task AddUserAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }
}
