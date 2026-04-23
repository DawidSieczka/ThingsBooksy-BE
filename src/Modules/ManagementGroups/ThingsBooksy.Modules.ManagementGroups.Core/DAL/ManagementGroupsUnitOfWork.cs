using ThingsBooksy.Shared.Infrastructure.Postgres;

namespace ThingsBooksy.Modules.ManagementGroups.Core.DAL;

internal class ManagementGroupsUnitOfWork : PostgresUnitOfWork<ManagementGroupsDbContext>
{
    public ManagementGroupsUnitOfWork(ManagementGroupsDbContext dbContext) : base(dbContext) { }
}
