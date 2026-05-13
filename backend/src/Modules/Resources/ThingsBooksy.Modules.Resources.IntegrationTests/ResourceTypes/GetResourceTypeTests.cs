using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.Resources.IntegrationTests.ResourceTypes;

/// <summary>
/// Integration tests for GET /resources/types/{id} and GET /resources/types?groupId={id} (T036–T037).
/// </summary>
[Collection("IntegrationTestCollection")]
public class GetResourceTypeTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly ResourcesUserFactory _users;
    private readonly ResourcesGroupReadModelFactory _groups;

    // Local response records — do not import from Core to keep test project isolated
    private record ResourceTypeDtoResponse(Guid Id, Guid GroupId, string Name, string? Description, DateTime CreatedAt, List<PropertyDefinitionDtoResponse> PropertyDefinitions);
    private record PropertyDefinitionDtoResponse(Guid Id, string Name, string DataType, bool IsRequired);
    private record ResourceTypesListResponse(List<ResourceTypeDtoResponse> Items);

    public GetResourceTypeTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ResourcesUserFactory(factory);
        _groups = new ResourcesGroupReadModelFactory(factory);
    }

    // -----------------------------------------------------------------------------------------
    // GET /resources/types/{id} — happy path
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task GetResourceType_AsOwner_Returns200WithCorrectData()
    {
        // Arrange
        var owner = await _users.CreateUserAsync("getrt_happy_owner@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);
        var client = new ResourcesTestClient(Factory, owner);

        var typeId = await client.CreateResourceTypeAndGetIdAsync(group.Id, "Desk", null,
            new[] { new PropertyDefinitionRequest("Color", (int)PropertyDataType.Text, true) });

        // Act
        var response = await client.GetResourceTypeAsync(typeId);

        // Assert — status
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert — body shape and values
        var body = await response.Content.ReadFromJsonAsync<ResourceTypeDtoResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(typeId, body.Id);
        Assert.Equal("Desk", body.Name);
        Assert.Equal(group.Id, body.GroupId);
        Assert.Single(body.PropertyDefinitions);
        Assert.Equal("Color", body.PropertyDefinitions[0].Name);
        Assert.Equal("Text", body.PropertyDefinitions[0].DataType);
        Assert.True(body.PropertyDefinitions[0].IsRequired);
    }

    // -----------------------------------------------------------------------------------------
    // GET /resources/types — happy path list (2 types)
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task GetResourceTypes_AsOwner_Returns200WithAllTypesInGroup()
    {
        // Arrange
        var owner = await _users.CreateUserAsync("getrtlist_happy_owner@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);
        var client = new ResourcesTestClient(Factory, owner);

        await client.CreateResourceTypeAndGetIdAsync(group.Id, "Chair");
        await client.CreateResourceTypeAndGetIdAsync(group.Id, "Table");

        // Act
        var response = await client.GetResourceTypesAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<ResourceTypeDtoResponse>>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);

        var names = body.ConvertAll(t => t.Name);
        Assert.Contains("Chair", names);
        Assert.Contains("Table", names);
    }

    // -----------------------------------------------------------------------------------------
    // GET /resources/types/{id} — 404 when type does not exist
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task GetResourceType_WhenTypeDoesNotExist_Returns404()
    {
        // Arrange
        var owner = await _users.CreateUserAsync("getrt_notfound@test.com");
        var client = new ResourcesTestClient(Factory, owner);

        var unknownId = Guid.CreateVersion7();

        // Act
        var response = await client.GetResourceTypeAsync(unknownId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -----------------------------------------------------------------------------------------
    // GET /resources/types/{id} — 401 when no JWT
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task GetResourceType_WithoutJwt_Returns401()
    {
        // Arrange — unauthenticated client
        var anonClient = Factory.CreateClient();
        var unknownId = Guid.CreateVersion7();

        // Act
        var response = await anonClient.GetAsync($"/resources/types/{unknownId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -----------------------------------------------------------------------------------------
    // GET /resources/types — 401 when no JWT
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task GetResourceTypes_WithoutJwt_Returns401()
    {
        // Arrange — unauthenticated client
        var anonClient = Factory.CreateClient();

        // Act
        var response = await anonClient.GetAsync($"/resources/types?groupId={Guid.CreateVersion7()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -----------------------------------------------------------------------------------------
    // GET /resources/types/{id} — 403 when user is not owner or member
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task GetResourceType_AsNonMember_Returns403()
    {
        // Arrange — resource type belongs to a group owned by someone else
        var owner = await _users.CreateUserAsync("getrt_403_owner@test.com");
        var nonMember = await _users.CreateUserAsync("getrt_403_nonmember@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);

        var ownerClient = new ResourcesTestClient(Factory, owner);
        var typeId = await ownerClient.CreateResourceTypeAndGetIdAsync(group.Id, "Bookshelf");

        var nonMemberClient = new ResourcesTestClient(Factory, nonMember);

        // Act
        var response = await nonMemberClient.GetResourceTypeAsync(typeId);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // -----------------------------------------------------------------------------------------
    // GET /resources/types — 403 when user is not owner or member
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task GetResourceTypes_AsNonMember_Returns403()
    {
        // Arrange
        var owner = await _users.CreateUserAsync("getrtlist_403_owner@test.com");
        var nonMember = await _users.CreateUserAsync("getrtlist_403_nonmember@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);

        var nonMemberClient = new ResourcesTestClient(Factory, nonMember);

        // Act
        var response = await nonMemberClient.GetResourceTypesAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // -----------------------------------------------------------------------------------------
    // GET /resources/types/{id} — 200 when requester is a group member (not owner)
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task GetResourceType_AsMember_Returns200()
    {
        // Arrange
        var owner = await _users.CreateUserAsync("getrt_member_owner@test.com");
        var member = await _users.CreateUserAsync("getrt_member_user@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);
        await _groups.AddGroupMemberAsync(group.Id, member.UserId);

        var ownerClient = new ResourcesTestClient(Factory, owner);
        var typeId = await ownerClient.CreateResourceTypeAndGetIdAsync(group.Id, "Whiteboard");

        var memberClient = new ResourcesTestClient(Factory, member);

        // Act
        var response = await memberClient.GetResourceTypeAsync(typeId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ResourceTypeDtoResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(typeId, body.Id);
    }

    // -----------------------------------------------------------------------------------------
    // GET /resources/types — 200 when requester is a group member (not owner)
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task GetResourceTypes_AsMember_Returns200()
    {
        // Arrange
        var owner = await _users.CreateUserAsync("getrtlist_member_owner@test.com");
        var member = await _users.CreateUserAsync("getrtlist_member_user@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);
        await _groups.AddGroupMemberAsync(group.Id, member.UserId);

        var ownerClient = new ResourcesTestClient(Factory, owner);
        await ownerClient.CreateResourceTypeAndGetIdAsync(group.Id, "Projector");

        var memberClient = new ResourcesTestClient(Factory, member);

        // Act
        var response = await memberClient.GetResourceTypesAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ResourceTypeDtoResponse>>(JsonOptions);
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("Projector", body[0].Name);
    }
}
