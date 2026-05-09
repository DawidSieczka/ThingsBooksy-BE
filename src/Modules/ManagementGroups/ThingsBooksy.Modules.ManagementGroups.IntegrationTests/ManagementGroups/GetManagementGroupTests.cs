using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class GetManagementGroupTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public GetManagementGroupTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    [Fact]
    public async Task GetGroup_AsOwner_Returns200WithMembers()
    {
        var owner = await _users.CreateUserAsync("getgroup_owner@test.com");
        var member = await _users.CreateUserAsync("getgroup_member@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Detailed Group");
        await groups.AddMemberAsync(groupId, member.Email);

        var response = await groups.GetGroupAsync(groupId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<ManagementGroupDetailDto>();
        Assert.NotNull(detail);
        Assert.Equal("Detailed Group", detail.Name);
        Assert.Single(detail.Members);
    }

    [Fact]
    public async Task GetGroup_AsMember_Returns200()
    {
        var owner = await _users.CreateUserAsync("getgroup_owner2@test.com");
        var member = await _users.CreateUserAsync("getgroup_member2@test.com");
        var ownerGroups = new ManagementGroupsTestClient(Factory, owner);
        var memberGroups = new ManagementGroupsTestClient(Factory, member);
        var groupId = await ownerGroups.CreateGroupAndGetIdAsync("Member Group");
        await ownerGroups.AddMemberAsync(groupId, member.Email);

        var response = await memberGroups.GetGroupAsync(groupId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetGroup_AsNonMember_Returns403()
    {
        var owner = await _users.CreateUserAsync("getgroup_owner3@test.com");
        var stranger = await _users.CreateUserAsync("getgroup_stranger@test.com");
        var ownerGroups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await ownerGroups.CreateGroupAndGetIdAsync("Private Group");

        var strangerGroups = new ManagementGroupsTestClient(Factory, stranger);
        var response = await strangerGroups.GetGroupAsync(groupId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetGroup_WhenSoftDeleted_Returns404()
    {
        var owner = await _users.CreateUserAsync("getgroup_deleted@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Group to Delete");
        await groups.DeleteGroupAsync(groupId);

        var response = await groups.GetGroupAsync(groupId);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
