using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class GetGroupMembersTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public GetGroupMembersTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    // T061 — first page of members always lists the owner first with isOwner:true
    [Fact]
    public async Task GetGroupMembers_FirstPage_IncludesOwnerFirst()
    {
        var owner = await _users.CreateUserAsync("getmembers_owner@test.com");
        var member1 = await _users.CreateUserAsync("getmembers_m1@test.com");
        var member2 = await _users.CreateUserAsync("getmembers_m2@test.com");
        var ownerClient = new ManagementGroupsTestClient(Factory, owner);

        var groupId = await ownerClient.CreateGroupAndGetIdAsync("Members Test Group");
        await ownerClient.AddMemberAsync(groupId, member1.Email);
        await ownerClient.AddMemberAsync(groupId, member2.Email);

        var response = await ownerClient.GetGroupMembersAsync(groupId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GroupMembersPage>(JsonOptions);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);

        // Owner is always first
        var first = result.Items[0];
        Assert.True(first.IsOwner);
        Assert.Equal(owner.UserId, first.UserId);

        // Other items are non-owner members
        var nonOwners = result.Items.Where(i => !i.IsOwner).ToList();
        Assert.Equal(2, nonOwners.Count);
        Assert.Contains(nonOwners, m => m.UserId == member1.UserId);
        Assert.Contains(nonOwners, m => m.UserId == member2.UserId);
    }

    // T062 — cursor pagination over 25 members (page size 20) produces no duplicates and
    //         covers all 25 members plus the owner; also verifies that members whose UserId
    //         is lower than the owner's UserId are still returned on subsequent pages
    //         (the cursor tracks members only, not the owner row).
    [Fact]
    public async Task GetGroupMembers_CursorPagination_NoDuplicatesAcrossPages()
    {
        var owner = await _users.CreateUserAsync("getmembers_cursor_owner@test.com");
        var ownerClient = new ManagementGroupsTestClient(Factory, owner);
        var groupId = await ownerClient.CreateGroupAndGetIdAsync("Large Members Group");

        // Seed 25 members
        var memberUserIds = new List<Guid>();
        for (var i = 1; i <= 25; i++)
        {
            var member = await _users.CreateUserAsync($"getmembers_cursor_m{i:D2}@test.com");
            await ownerClient.AddMemberAsync(groupId, member.Email);
            memberUserIds.Add(member.UserId);
        }

        // Page 1 — no cursor
        var page1Response = await ownerClient.GetGroupMembersAsync(groupId, take: 20);
        Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);
        var page1 = await page1Response.Content.ReadFromJsonAsync<GroupMembersPage>(JsonOptions);
        Assert.NotNull(page1);
        Assert.Equal(20, page1.Items.Count);
        Assert.NotNull(page1.NextCursor); // More pages exist

        // Page 2 — use NextCursor from page 1
        var page2Response = await ownerClient.GetGroupMembersAsync(groupId, afterId: page1.NextCursor, take: 20);
        Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);
        var page2 = await page2Response.Content.ReadFromJsonAsync<GroupMembersPage>(JsonOptions);
        Assert.NotNull(page2);
        Assert.True(page2.Items.Count > 0);
        Assert.Null(page2.NextCursor); // Last page

        // Combine pages and verify
        var allItems = page1.Items.Concat(page2.Items).ToList();

        // Total: owner (1) + 25 members = 26
        Assert.Equal(26, allItems.Count);

        // No duplicates
        var allUserIds = allItems.Select(i => i.UserId).ToList();
        Assert.Equal(allUserIds.Count, allUserIds.Distinct().Count());

        // Owner appears exactly once and only on page 1 as first item
        Assert.True(page1.Items[0].IsOwner);
        Assert.Equal(owner.UserId, page1.Items[0].UserId);
        Assert.DoesNotContain(page2.Items, i => i.IsOwner);

        // All 25 member UserIds are present across both pages
        var nonOwnerUserIds = allItems
            .Where(i => !i.IsOwner)
            .Select(i => i.UserId)
            .ToHashSet();
        foreach (var memberId in memberUserIds)
        {
            Assert.Contains(memberId, nonOwnerUserIds);
        }
    }

    // T063 — a user who is neither owner nor member receives 403
    [Fact]
    public async Task GetGroupMembers_NonMemberCaller_Returns403()
    {
        var owner = await _users.CreateUserAsync("getmembers_nonmember_owner@test.com");
        var stranger = await _users.CreateUserAsync("getmembers_nonmember_stranger@test.com");
        var ownerClient = new ManagementGroupsTestClient(Factory, owner);
        var strangerClient = new ManagementGroupsTestClient(Factory, stranger);

        var groupId = await ownerClient.CreateGroupAndGetIdAsync("Private Group");

        var response = await strangerClient.GetGroupMembersAsync(groupId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // Response shape DTOs — test-only, not imported from Core (internal types)
    // ---------------------------------------------------------------------------

    private sealed record GroupMembersPage(
        List<MemberItemDto> Items,
        Guid? NextCursor);

    private sealed record MemberItemDto(
        Guid UserId,
        string Email,
        DateTimeOffset JoinedAt,
        bool IsOwner);
}
