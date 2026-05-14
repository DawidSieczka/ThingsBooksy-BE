using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    // T066 — renaming a group to a name already taken by a different group of the same owner returns 409
    [Fact]
    public async Task UpdateManagementGroup_DuplicateNameAmongOthers_Returns409()
    {
        var owner = await _users.CreateUserAsync("update_dup_owner@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);

        await groups.CreateGroupAndGetIdAsync("Existing Name");
        var targetId = await groups.CreateGroupAndGetIdAsync("Target Group");

        var response = await groups.UpdateGroupAsync(targetId, "Existing Name");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var body = await response.Content.ReadFromJsonAsync<UpdateErrorBody>(options);
        Assert.NotNull(body);
        Assert.Equal("GROUP_NAME_TAKEN", body.Code);

        // Assert no side effect — group retains original name
        var dbGroup = await groups.GetGroupFromDbAsync(targetId);
        Assert.NotNull(dbGroup);
        Assert.Equal("Target Group", dbGroup.Name);
    }

    // T067 — updating a group with its own current name (no conflict with itself) succeeds
    [Fact]
    public async Task UpdateManagementGroup_SameNameAsItself_Succeeds()
    {
        var owner = await _users.CreateUserAsync("update_selfname@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("My Group");

        var response = await groups.UpdateGroupAsync(groupId, "My Group", "Updated description");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dbGroup = await groups.GetGroupFromDbAsync(groupId);
        Assert.NotNull(dbGroup);
        Assert.Equal("My Group", dbGroup.Name);
        Assert.Equal("Updated description", dbGroup.Description);
    }

    // DTO used only within this test file to deserialise the 409 body
    private sealed record UpdateErrorBody(string Code, string Message);
}
