using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class GetManagementGroupsTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public GetManagementGroupsTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    [Fact]
    public async Task GetGroups_ReturnsOnlyCurrentUserGroups_Returns200()
    {
        var owner = await _users.CreateUserAsync("getgroups_owner@test.com");
        var other = await _users.CreateUserAsync("getgroups_other@test.com");
        var ownerGroups = new ManagementGroupsTestClient(Factory, owner);
        var otherGroups = new ManagementGroupsTestClient(Factory, other);
        await ownerGroups.CreateGroupAndGetIdAsync("Owner Group 1");
        await ownerGroups.CreateGroupAndGetIdAsync("Owner Group 2");
        await otherGroups.CreateGroupAndGetIdAsync("Other Group");

        var response = await ownerGroups.GetGroupsAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var groups = await response.Content.ReadFromJsonAsync<List<GetManagementGroupsQueryResult>>();
        Assert.NotNull(groups);
        Assert.Equal(2, groups.Count);
        Assert.All(groups, g => Assert.Equal(owner.UserId, g.OwnerId));
    }

    [Fact]
    public async Task GetGroups_DoesNotReturnDeletedGroups()
    {
        var owner = await _users.CreateUserAsync("getgroups_deleted@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        await groups.CreateGroupAndGetIdAsync("Active Group");
        var deletedId = await groups.CreateGroupAndGetIdAsync("Deleted Group");
        await groups.DeleteGroupAsync(deletedId);

        var response = await groups.GetGroupsAsync();

        var result = await response.Content.ReadFromJsonAsync<List<GetManagementGroupsQueryResult>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Active Group", result[0].Name);
    }

    [Fact]
    public async Task GetGroups_IncludesGroupsWhereUserIsMember()
    {
        var owner = await _users.CreateUserAsync("getgroups_mem_owner@test.com");
        var member = await _users.CreateUserAsync("getgroups_mem_member@test.com");
        var ownerGroups = new ManagementGroupsTestClient(Factory, owner);
        var memberGroups = new ManagementGroupsTestClient(Factory, member);
        var groupId = await ownerGroups.CreateGroupAndGetIdAsync("Shared Group");
        await ownerGroups.AddMemberAsync(groupId, member.Email);

        var response = await memberGroups.GetGroupsAsync();

        var result = await response.Content.ReadFromJsonAsync<List<GetManagementGroupsQueryResult>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Shared Group", result[0].Name);
    }
}
