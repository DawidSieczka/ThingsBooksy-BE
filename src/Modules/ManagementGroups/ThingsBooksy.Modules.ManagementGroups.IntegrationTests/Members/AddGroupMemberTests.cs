using System;
using System.Net;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class AddGroupMemberTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public AddGroupMemberTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    [Fact]
    public async Task AddMember_WithValidEmail_Returns201AndPersistsInDb()
    {
        var owner = await _users.CreateUserAsync("addmember_owner@test.com");
        var member = await _users.CreateUserAsync("addmember_member@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Group With Members");

        var response = await groups.AddMemberAsync(groupId, member.Email);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dbMembers = await groups.GetMembersFromDbAsync(groupId);
        Assert.Single(dbMembers);
        Assert.Equal(member.UserId, dbMembers[0].UserId);
    }

    [Fact]
    public async Task AddMember_WhenUserNotExists_Returns404()
    {
        var owner = await _users.CreateUserAsync("addmember_notexists@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Group");

        var response = await groups.AddMemberAsync(groupId, "ghost@test.com");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var dbMembers = await groups.GetMembersFromDbAsync(groupId);
        Assert.Empty(dbMembers);
    }

    [Fact]
    public async Task AddMember_WhenAlreadyMember_Returns400()
    {
        var owner = await _users.CreateUserAsync("addmember_dup_owner@test.com");
        var member = await _users.CreateUserAsync("addmember_dup_member@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Group");
        await groups.AddMemberAsync(groupId, member.Email);

        var response = await groups.AddMemberAsync(groupId, member.Email);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_AsNonOwner_Returns403()
    {
        var owner = await _users.CreateUserAsync("addmember_nonowner_owner@test.com");
        var nonOwner = await _users.CreateUserAsync("addmember_nonowner_user@test.com");
        var candidate = await _users.CreateUserAsync("addmember_nonowner_cand@test.com");
        var ownerGroups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await ownerGroups.CreateGroupAndGetIdAsync("Group");

        var nonOwnerGroups = new ManagementGroupsTestClient(Factory, nonOwner);
        var response = await nonOwnerGroups.AddMemberAsync(groupId, candidate.Email);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_OwnerAddsHimself_Returns400()
    {
        var owner = await _users.CreateUserAsync("addmember_selfadd@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Group");

        var response = await groups.AddMemberAsync(groupId, owner.Email);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_WithInvalidEmailFormat_Returns400()
    {
        var owner = await _users.CreateUserAsync("addmember_invalidemail@test.com");
        var groups = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await groups.CreateGroupAndGetIdAsync("Group");

        var response = await groups.AddMemberAsync(groupId, "not-an-email");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
