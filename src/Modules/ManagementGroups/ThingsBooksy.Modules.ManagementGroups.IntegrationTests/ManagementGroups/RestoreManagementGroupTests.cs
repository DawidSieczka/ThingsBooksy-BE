using System.Net;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class RestoreManagementGroupTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public RestoreManagementGroupTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    [Fact]
    public async Task RestoreGroup_AsOwner_Returns200AndClearsDeletedAt()
    {
        var owner = await _users.CreateUserAsync("restore_owner@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Group to Restore");
        await groups.DeleteGroupAsync(groupId);

        var response = await groups.RestoreGroupAsync(groupId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dbGroup = await groups.GetGroupFromDbAsync(groupId);
        Assert.NotNull(dbGroup);
        Assert.Null(dbGroup.DeletedAt);
    }

    [Fact]
    public async Task RestoreGroup_WhenNotDeleted_Returns400()
    {
        var owner = await _users.CreateUserAsync("restore_notdeleted@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Active Group");

        var response = await groups.RestoreGroupAsync(groupId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RestoreGroup_AfterRestore_IsVisibleAgain()
    {
        var owner = await _users.CreateUserAsync("restore_visible@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Restored Group");
        await groups.DeleteGroupAsync(groupId);
        await groups.RestoreGroupAsync(groupId);

        var getResponse = await groups.GetGroupAsync(groupId);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
}
