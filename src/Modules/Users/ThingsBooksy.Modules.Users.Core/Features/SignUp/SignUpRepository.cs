using System.Threading;
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

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _dbContext.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<Role?> GetRoleAsync(string roleName, CancellationToken cancellationToken = default)
        => _dbContext.Roles.SingleOrDefaultAsync(r => r.Name == roleName, cancellationToken);

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
