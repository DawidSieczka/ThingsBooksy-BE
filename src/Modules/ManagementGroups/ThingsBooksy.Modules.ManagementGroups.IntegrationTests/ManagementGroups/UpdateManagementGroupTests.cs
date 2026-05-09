using System.Net;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class UpdateManagementGroupTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public UpdateManagementGroupTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    [Fact]
    public async Task UpdateGroup_AsOwner_Returns200AndUpdatesDb()
    {
        var owner = await _users.CreateUserAsync("update_owner@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Original Name");

        var response = await groups.UpdateGroupAsync(groupId, "Updated Name", "New description");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dbGroup = await groups.GetGroupFromDbAsync(groupId);
        Assert.NotNull(dbGroup);
        Assert.Equal("Updated Name", dbGroup.Name);
        Assert.Equal("New description", dbGroup.Description);
    }

    [Fact]
    public async Task UpdateGroup_AsNonOwner_Returns403()
    {
        var owner = await _users.CreateUserAsync("update_owner2@test.com");
        var ownerGroups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await ownerGroups.CreateGroupAndGetIdAsync("Owner's Group");

        var other = await _users.CreateUserAsync("update_other@test.com");
        var otherGroups = new ManagementGroupsTestClient(Factory, other);

        var response = await otherGroups.UpdateGroupAsync(groupId, "Hijacked Name");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
