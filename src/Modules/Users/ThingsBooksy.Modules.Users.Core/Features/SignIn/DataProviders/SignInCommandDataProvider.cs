using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.SignIn.DataProviders;

internal sealed class SignInCommandDataProvider : ISignInCommandDataProvider
{
    private readonly UsersDbContext _dbContext;

    public SignInCommandDataProvider(UsersDbContext dbContext)
        => _dbContext = dbContext;

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _dbContext.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
}
