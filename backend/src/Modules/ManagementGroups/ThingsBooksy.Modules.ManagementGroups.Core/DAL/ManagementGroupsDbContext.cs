using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;
using ThingsBooksy.Shared.Infrastructure.Messaging.Outbox;

namespace ThingsBooksy.Modules.ManagementGroups.Core.DAL;

internal class ManagementGroupsDbContext : DbContext
{
    public DbSet<ManagementGroup> ManagementGroups { get; set; } = null!;
    public DbSet<GroupMember> GroupMembers { get; set; } = null!;
    public DbSet<UserReadModel> UserReadModels { get; set; } = null!;
    public DbSet<InboxMessage> Inbox { get; set; } = null!;
    public DbSet<OutboxMessage> Outbox { get; set; } = null!;

    public ManagementGroupsDbContext(DbContextOptions<ManagementGroupsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("management_groups");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
