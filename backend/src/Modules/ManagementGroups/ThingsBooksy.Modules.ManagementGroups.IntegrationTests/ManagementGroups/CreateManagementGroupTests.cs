using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class CreateManagementGroupTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public CreateManagementGroupTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    [Fact]
    public async Task CreateGroup_WithValidData_Returns201AndPersistsInDb()
    {
        var owner = await _users.CreateUserAsync("create_valid@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);

        var response = await groups.CreateGroupAsync("My Group");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateGroupResponse>();
        Assert.NotNull(result);

        var dbGroup = await groups.GetGroupFromDbAsync(result.Id);
        Assert.NotNull(dbGroup);
        Assert.Equal("My Group", dbGroup.Name);
        Assert.Equal(owner.UserId, dbGroup.OwnerId);
    }

    [Fact]
    public async Task CreateGroup_WhenUnauthenticated_Returns401()
    {
        var anonymousClient = Factory.CreateClient();

        var response = await anonymousClient.PostAsJsonAsync("/management-groups", new { Name = "Anon Group" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateGroup_WithDuplicateName_Returns400()
    {
        var owner = await _users.CreateUserAsync("create_dup@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        await groups.CreateGroupAndGetIdAsync("Duplicate Name");

        var response = await groups.CreateGroupAsync("Duplicate Name");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateGroup_WithoutName_Returns400()
    {
        var owner = await _users.CreateUserAsync("create_noname@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);

        var response = await groups.CreateGroupAsync("");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
