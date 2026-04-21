using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThingsBooksy.Modules.Users.Core.Entities;
using ThingsBooksy.Shared.Infrastructure;

namespace ThingsBooksy.Modules.Users.Core.DAL;

internal sealed class UsersInitializer : IInitializer
{
    private readonly HashSet<string> _permissions = ["users"];

    private readonly UsersDbContext _dbContext;
    private readonly ILogger<UsersInitializer> _logger;

    public UsersInitializer(UsersDbContext dbContext, ILogger<UsersInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task InitAsync()
    {
        if (await _dbContext.Roles.AnyAsync())
            return;

        await _dbContext.Roles.AddAsync(new Role { Name = Role.Admin, Permissions = _permissions });
        await _dbContext.Roles.AddAsync(new Role { Name = Role.User, Permissions = new List<string>() });
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Initialized Users roles.");
    }
}
