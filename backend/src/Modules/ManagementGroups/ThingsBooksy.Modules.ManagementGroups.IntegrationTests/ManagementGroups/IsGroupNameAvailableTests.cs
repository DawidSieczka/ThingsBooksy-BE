using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests;

public class IsGroupNameAvailableTests : IntegrationTestBase
{
    private readonly ManagementGroupsUserFactory _users;

    public IsGroupNameAvailableTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ManagementGroupsUserFactory(factory);
    }

    // T028 — querying a name that the caller does not own yet returns available: true
    [Fact]
    public async Task IsGroupNameAvailable_AvailableName_ReturnsTrue()
    {
        var user = await _users.CreateUserAsync("nameavail_available@test.com");
        var client = new ManagementGroupsTestClient(Factory, user);

        var response = await client.IsGroupNameAvailableAsync("Brand New Name");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var body = await response.Content.ReadFromJsonAsync<AvailabilityResult>(options);
        Assert.NotNull(body);
        Assert.True(body.Available);
    }

    // T029 — querying a name the caller already owns returns available: false
    [Fact]
    public async Task IsGroupNameAvailable_TakenName_ReturnsFalse()
    {
        var user = await _users.CreateUserAsync("nameavail_taken@test.com");
        var client = new ManagementGroupsTestClient(Factory, user);
        await client.CreateGroupAndGetIdAsync("Already Taken");

        var response = await client.IsGroupNameAvailableAsync("Already Taken");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var body = await response.Content.ReadFromJsonAsync<AvailabilityResult>(options);
        Assert.NotNull(body);
        Assert.False(body.Available);
    }

    // T030 — soft-deleted group's name is treated as available (soft-deleted groups don't count)
    [Fact]
    public async Task IsGroupNameAvailable_NameOfSoftDeletedGroup_ReturnsTrue()
    {
        var user = await _users.CreateUserAsync("nameavail_softdel@test.com");
        var client = new ManagementGroupsTestClient(Factory, user);

        var groupId = await client.CreateGroupAndGetIdAsync("Deleted Group Name");
        await client.DeleteGroupAsync(groupId);

        var response = await client.IsGroupNameAvailableAsync("Deleted Group Name");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var body = await response.Content.ReadFromJsonAsync<AvailabilityResult>(options);
        Assert.NotNull(body);
        Assert.True(body.Available);
    }

    // T031 — availability check is scoped per owner; a name taken by a different owner is still available for the caller
    [Fact]
    public async Task IsGroupNameAvailable_DifferentOwner_NameTaken_ReturnsTrue()
    {
        var owner1 = await _users.CreateUserAsync("nameavail_owner1@test.com");
        var owner2 = await _users.CreateUserAsync("nameavail_owner2@test.com");

        var groups1 = new ManagementGroupsTestClient(Factory, owner1);
        await groups1.CreateGroupAndGetIdAsync("Cross Owner Name");

        // owner2 has NOT created any group named "Cross Owner Name" — should be available to them
        var groups2 = new ManagementGroupsTestClient(Factory, owner2);
        var response = await groups2.IsGroupNameAvailableAsync("Cross Owner Name");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var body = await response.Content.ReadFromJsonAsync<AvailabilityResult>(options);
        Assert.NotNull(body);
        Assert.True(body.Available);
    }

    // DTO used only within this file to deserialise the response body
    private sealed record AvailabilityResult(bool Available);
}
