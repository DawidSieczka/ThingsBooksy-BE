using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

/// <summary>
/// Verifies that each ManagementGroups command handler publishes the expected integration event
/// (T005–T008). Assertions inspect the Resources module's read-model tables via raw SQL —
/// populating those tables is the concrete side-effect that proves the event was published and
/// handled end-to-end by the Resources module's event handlers.
///
/// Because events are dispatched asynchronously via AsyncDispatcherJob (an in-process background
/// service reading from a Channel), each assertion polls until the side-effect is visible or a
/// 3-second deadline elapses.
/// </summary>
public class EventPublishingTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public EventPublishingTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    // -----------------------------------------------------------------------------------------
    // T005 — CreateManagementGroupHandler publishes GroupCreated
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task CreateGroup_WithValidData_PublishesGroupCreatedEvent()
    {
        var owner = await _users.CreateUserAsync("eventpub_create_owner@test.com");
        var client = new ManagementGroupsTestClient(Factory, owner);

        var response = await client.CreateGroupAsync("EventPub Create Group");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateGroupResponse>();
        Assert.NotNull(result);
        var groupId = result.Id;

        // GroupCreated event triggers GroupCreatedHandler in Resources which upserts GroupReadModel.
        var populated = await WaitUntilAsync(
            () => client.ResourcesGroupReadModelExistsAsync(groupId));

        Assert.True(populated, "Resources.GroupReadModel was not populated after CreateManagementGroup — GroupCreated event was not published or handled.");
    }

    // -----------------------------------------------------------------------------------------
    // T006 — DeleteManagementGroupHandler publishes GroupDeleted
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task DeleteGroup_AsOwner_PublishesGroupDeletedEvent()
    {
        var owner = await _users.CreateUserAsync("eventpub_delete_owner@test.com");
        var client = new ManagementGroupsTestClient(Factory, owner);

        var groupId = await client.CreateGroupAndGetIdAsync("EventPub Delete Group");

        // Pre-condition: GroupCreated event must have populated the read-model before we test deletion.
        var rowCreated = await WaitUntilAsync(() => client.ResourcesGroupReadModelExistsAsync(groupId));
        Assert.True(rowCreated, "Pre-condition failed: Resources.GroupReadModel was not populated after CreateManagementGroup. Cannot test GroupDeleted event propagation.");

        var deleteResponse = await client.DeleteGroupAsync(groupId);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // GroupDeleted event triggers GroupDeletedHandler in Resources which removes GroupReadModel.
        var removed = await WaitUntilAsync(
            () => client.ResourcesGroupReadModelAbsentAsync(groupId));

        Assert.True(removed, "Resources.GroupReadModel was not removed after DeleteManagementGroup — GroupDeleted event was not published or handled.");
    }

    // -----------------------------------------------------------------------------------------
    // T007 — AddGroupMemberHandler publishes GroupMemberAdded
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task AddMember_WithValidEmail_PublishesGroupMemberAddedEvent()
    {
        var owner = await _users.CreateUserAsync("eventpub_addmember_owner@test.com");
        var member = await _users.CreateUserAsync("eventpub_addmember_member@test.com");
        var client = new ManagementGroupsTestClient(Factory, owner);

        var groupId = await client.CreateGroupAndGetIdAsync("EventPub AddMember Group");

        var addResponse = await client.AddMemberAsync(groupId, member.Email);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);

        // GroupMemberAdded event triggers GroupMemberAddedHandler in Resources which inserts GroupMemberReadModel.
        var populated = await WaitUntilAsync(
            () => client.ResourcesGroupMemberReadModelExistsAsync(groupId, member.UserId));

        Assert.True(populated, "Resources.GroupMemberReadModel was not populated after AddGroupMember — GroupMemberAdded event was not published or handled.");
    }

    // -----------------------------------------------------------------------------------------
    // T008 — RemoveGroupMemberHandler publishes GroupMemberRemoved
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task RemoveMember_AsOwner_PublishesGroupMemberRemovedEvent()
    {
        var owner = await _users.CreateUserAsync("eventpub_removemember_owner@test.com");
        var member = await _users.CreateUserAsync("eventpub_removemember_member@test.com");
        var client = new ManagementGroupsTestClient(Factory, owner);

        var groupId = await client.CreateGroupAndGetIdAsync("EventPub RemoveMember Group");
        await client.AddMemberAsync(groupId, member.Email);

        // Pre-condition: GroupMemberAdded event must have populated the read-model before we test removal.
        var rowCreated = await WaitUntilAsync(() => client.ResourcesGroupMemberReadModelExistsAsync(groupId, member.UserId));
        Assert.True(rowCreated, "Pre-condition failed: Resources.GroupMemberReadModel was not populated after AddGroupMember. Cannot test GroupMemberRemoved event propagation.");

        var removeResponse = await client.RemoveMemberAsync(groupId, member.UserId);
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        // GroupMemberRemoved event triggers GroupMemberRemovedHandler in Resources which deletes GroupMemberReadModel.
        var removed = await WaitUntilAsync(
            () => client.ResourcesGroupMemberReadModelAbsentAsync(groupId, member.UserId));

        Assert.True(removed, "Resources.GroupMemberReadModel was not removed after RemoveGroupMember — GroupMemberRemoved event was not published or handled.");
    }

    // -----------------------------------------------------------------------------------------
    // Polling helper — retries the condition every 100 ms for up to 3 seconds
    // -----------------------------------------------------------------------------------------

    private static async Task<bool> WaitUntilAsync(
        Func<Task<bool>> condition,
        int maxAttempts = 30,
        int intervalMs = 100)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            if (await condition())
                return true;

            await Task.Delay(intervalMs);
        }

        return false;
    }
}
