using System.Net;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class DeleteManagementGroupTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public DeleteManagementGroupTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    [Fact]
    public async Task DeleteGroup_AsOwner_Returns204AndSoftDeletesInDb()
    {
        var owner = await _users.CreateUserAsync("delete_owner@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Group to Delete");

        var response = await groups.DeleteGroupAsync(groupId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var dbGroup = await groups.GetGroupFromDbAsync(groupId);
        Assert.NotNull(dbGroup);
        Assert.NotNull(dbGroup.DeletedAt);
    }

    [Fact]
    public async Task DeleteGroup_AsNonOwner_Returns403()
    {
        var owner = await _users.CreateUserAsync("delete_owner2@test.com");
        var ownerGroups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await ownerGroups.CreateGroupAndGetIdAsync("Protected Group");

        var other = await _users.CreateUserAsync("delete_other@test.com");
        var otherGroups = new ManagementGroupsTestClient(Factory, other);

        var response = await otherGroups.DeleteGroupAsync(groupId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteGroup_MakesGroupInvisibleInGetRequests()
    {
        var owner = await _users.CreateUserAsync("delete_invisible@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Invisible After Delete");

        await groups.DeleteGroupAsync(groupId);

        var getResponse = await groups.GetGroupAsync(groupId);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
