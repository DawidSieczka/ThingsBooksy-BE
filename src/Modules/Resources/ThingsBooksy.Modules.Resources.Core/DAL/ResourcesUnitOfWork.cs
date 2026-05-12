using ThingsBooksy.Shared.Infrastructure.Postgres;

namespace ThingsBooksy.Modules.Resources.Core.DAL;

internal class ResourcesUnitOfWork : PostgresUnitOfWork<ResourcesDbContext>
{
    public ResourcesUnitOfWork(ResourcesDbContext dbContext) : base(dbContext) { }
}
