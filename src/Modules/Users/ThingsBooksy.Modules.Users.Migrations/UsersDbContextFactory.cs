using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ThingsBooksy.Modules.Users.Core.DAL;

namespace ThingsBooksy.Modules.Users.Migrations;

/// <summary>
/// Design-time factory used by EF Core tooling (dotnet ef migrations add).
/// Usage: dotnet ef migrations add <Name> --project Users.Migrations --startup-project Bootstrapper
/// </summary>
internal sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration["postgres:connectionString"]
            ?? throw new InvalidOperationException("Missing 'postgres:connectionString' in configuration.");

        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b =>
            b.MigrationsAssembly(typeof(UsersDbContextFactory).Assembly.GetName().Name));

        return new UsersDbContext(optionsBuilder.Options);
    }
}
