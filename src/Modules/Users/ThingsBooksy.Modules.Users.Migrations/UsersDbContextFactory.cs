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
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("postgres")
            ?? "Host=localhost;Database=thingsbooksy;Username=postgres;Password=";

        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b =>
            b.MigrationsAssembly(typeof(UsersDbContextFactory).Assembly.FullName));

        return new UsersDbContext(optionsBuilder.Options);
    }
}
