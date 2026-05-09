using System.Net;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class RemoveGroupMemberTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public RemoveGroupMemberTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    [Fact]
    public async Task RemoveMember_AsOwner_Returns204AndRemovesFromDb()
    {
        var owner = await _users.CreateUserAsync("removemember_owner@test.com");
        var member = await _users.CreateUserAsync("removemember_member@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Group");
        await groups.AddMemberAsync(groupId, member.Email);

        var response = await groups.RemoveMemberAsync(groupId, member.UserId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var dbMembers = await groups.GetMembersFromDbAsync(groupId);
        Assert.Empty(dbMembers);
    }

    [Fact]
    public async Task RemoveMember_AsNonOwner_Returns403()
    {
        var owner = await _users.CreateUserAsync("removemember_owner2@test.com");
        var member = await _users.CreateUserAsync("removemember_member2@test.com");
        var nonOwner = await _users.CreateUserAsync("removemember_nonowner@test.com");
        var ownerGroups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await ownerGroups.CreateGroupAndGetIdAsync("Group");
        await ownerGroups.AddMemberAsync(groupId, member.Email);

        var nonOwnerGroups = new ManagementGroupsTestClient(Factory, nonOwner);
        var response = await nonOwnerGroups.RemoveMemberAsync(groupId, member.UserId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
