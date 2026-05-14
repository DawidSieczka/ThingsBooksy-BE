using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.IntegrationTests.Clients;
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.Resources.IntegrationTests.ResourceTypes;

/// <summary>
/// Integration tests for PUT /resources/types/{id} and DELETE /resources/types/{id}.
///
/// GroupReadModel rows are inserted directly via ResourcesGroupReadModelFactory to avoid
/// depending on async event propagation from the ManagementGroups pipeline.
/// </summary>
[Collection("IntegrationTestCollection")]
public class UpdateDeleteResourceTypeTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private record InstanceSummary(Guid Id, string Name);
    private record PagedInstancesResponse(List<InstanceSummary> Items, Guid? NextCursor);

    private readonly ResourcesUserFactory _users;
    private readonly ResourcesGroupReadModelFactory _groups;

    public UpdateDeleteResourceTypeTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new ResourcesUserFactory(factory);
        _groups = new ResourcesGroupReadModelFactory(factory);
    }

    // -----------------------------------------------------------------------------------------
    // PUT /resources/types/{id} — happy path: name + description updated in DB
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task UpdateResourceType_AsOwner_Returns204AndUpdatesDb()
    {
        // Arrange
        var owner = await _users.CreateUserAsync("updrt_happy_owner@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);
        var client = new ResourcesTestClient(Factory, owner);

        var defs = new[]
        {
            new PropertyDefinitionRequest("Color", (int)PropertyDataType.Text, false),
        };
        var typeId = await client.CreateResourceTypeAndGetIdAsync(group.Id, "Original Name", "Original Desc", defs);

        // Build update payload: keep the existing definition (with its Id) and add a new one (no Id)
        var storedDefs = await client.GetResourcePropertyDefinitionsFromDbAsync(typeId);
        var existingDefId = storedDefs.First(d => d.Name == "Color").Id;

        var updateDefs = new[]
        {
            new PropertyDefinitionUpdateRequest(existingDefId, "Color", (int)PropertyDataType.Text, false),
            new PropertyDefinitionUpdateRequest(null, "Weight", (int)PropertyDataType.Number, true),
        };

        // Act
        var response = await client.UpdateResourceTypeAsync(
            typeId, "Updated Name", "Updated Desc", updateDefs);

        // Assert — 204 No Content
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Assert — name and description updated in DB
        var resourceType = await client.GetResourceTypeFromDbAsync(typeId);
        Assert.NotNull(resourceType);
        Assert.Equal("Updated Name", resourceType.Name);
        Assert.Equal("Updated Desc", resourceType.Description);

        // Assert — definitions reconciled: old one retained, new one added
        var definitionsAfter = await client.GetResourcePropertyDefinitionsFromDbAsync(typeId);
        Assert.Equal(2, definitionsAfter.Count);
        Assert.Contains(definitionsAfter, d => d.Name == "Color");
        Assert.Contains(definitionsAfter, d => d.Name == "Weight" && d.IsRequired);
    }

    // -----------------------------------------------------------------------------------------
    // PUT /resources/types/{id} — 401 Unauthenticated
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task UpdateResourceType_WithoutJwt_Returns401()
    {
        // Arrange — unauthenticated client
        var anonClient = Factory.CreateClient();

        // Act
        var response = await anonClient.PutAsJsonAsync($"/resources/types/{Guid.CreateVersion7()}", new
        {
            Name = "Attempt",
            Description = (string?)null,
            PropertyDefinitions = Array.Empty<object>()
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -----------------------------------------------------------------------------------------
    // PUT /resources/types/{id} — 403 Non-owner (authenticated but not the group owner)
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task UpdateResourceType_AsNonOwner_Returns403()
    {
        // Arrange — group is owned by owner; nonOwner is a group member but not owner
        var owner = await _users.CreateUserAsync("updrt_403_owner@test.com");
        var nonOwner = await _users.CreateUserAsync("updrt_403_nonowner@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);
        await _groups.AddGroupMemberAsync(group.Id, nonOwner.UserId);

        var ownerClient = new ResourcesTestClient(Factory, owner);
        var typeId = await ownerClient.CreateResourceTypeAndGetIdAsync(group.Id, "Type For 403 Test");

        var nonOwnerClient = new ResourcesTestClient(Factory, nonOwner);

        // Act
        var response = await nonOwnerClient.UpdateResourceTypeAsync(typeId, "Tampered Name");

        // Assert — 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // Assert — name unchanged in DB
        var resourceType = await ownerClient.GetResourceTypeFromDbAsync(typeId);
        Assert.NotNull(resourceType);
        Assert.Equal("Type For 403 Test", resourceType.Name);
    }

    // -----------------------------------------------------------------------------------------
    // PUT /resources/types/{id} — 400 Unknown resource type ID
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task UpdateResourceType_WithUnknownId_Returns400()
    {
        // Arrange
        var user = await _users.CreateUserAsync("updrt_unknownid@test.com");
        var client = new ResourcesTestClient(Factory, user);

        var unknownId = Guid.CreateVersion7();

        // Act
        var response = await client.UpdateResourceTypeAsync(unknownId, "Ghost Name");

        // Assert — resource type not found → ResourcesDomainException (CustomException) → 400
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Assert — no type row was created as a side-effect
        var resourceType = await client.GetResourceTypeFromDbAsync(unknownId);
        Assert.Null(resourceType);
    }

    // -----------------------------------------------------------------------------------------
    // DELETE /resources/types/{id} — happy path: 204 + hard-delete verified in DB
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task DeleteResourceType_AsOwner_Returns204AndRemovesFromDb()
    {
        // Arrange — create a type with no instances
        var owner = await _users.CreateUserAsync("delrt_happy_owner@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);
        var client = new ResourcesTestClient(Factory, owner);

        var typeId = await client.CreateResourceTypeAndGetIdAsync(group.Id, "Type To Delete", "Will be gone");

        // Act
        var response = await client.DeleteResourceTypeAsync(typeId);

        // Assert — 204 No Content
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Assert — hard-deleted: NOT found even with IgnoreQueryFilters()
        var resourceType = await client.GetResourceTypeFromDbAsync(typeId);
        Assert.Null(resourceType);

        // Assert — associated property definitions also removed (cascade)
        var definitions = await client.GetResourcePropertyDefinitionsFromDbAsync(typeId);
        Assert.Empty(definitions);
    }

    // -----------------------------------------------------------------------------------------
    // DELETE /resources/types/{id} — 401 Unauthenticated
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task DeleteResourceType_WithoutJwt_Returns401()
    {
        // Arrange — unauthenticated client
        var anonClient = Factory.CreateClient();

        // Act
        var response = await anonClient.DeleteAsync($"/resources/types/{Guid.CreateVersion7()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -----------------------------------------------------------------------------------------
    // DELETE /resources/types/{id} — 403 Non-owner (authenticated but not the group owner)
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task DeleteResourceType_AsNonOwner_Returns403()
    {
        // Arrange
        var owner = await _users.CreateUserAsync("delrt_403_owner@test.com");
        var nonOwner = await _users.CreateUserAsync("delrt_403_nonowner@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);
        await _groups.AddGroupMemberAsync(group.Id, nonOwner.UserId);

        var ownerClient = new ResourcesTestClient(Factory, owner);
        var typeId = await ownerClient.CreateResourceTypeAndGetIdAsync(group.Id, "Type For Delete 403 Test");

        var nonOwnerClient = new ResourcesTestClient(Factory, nonOwner);

        // Act
        var response = await nonOwnerClient.DeleteResourceTypeAsync(typeId);

        // Assert — 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // Assert — type still exists in DB (not deleted)
        var resourceType = await ownerClient.GetResourceTypeFromDbAsync(typeId);
        Assert.NotNull(resourceType);
    }

    // -----------------------------------------------------------------------------------------
    // DELETE /resources/types/{id} — 400 Unknown resource type ID
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task DeleteResourceType_WithUnknownId_Returns400()
    {
        // Arrange
        var user = await _users.CreateUserAsync("delrt_unknownid@test.com");
        var client = new ResourcesTestClient(Factory, user);

        var unknownId = Guid.CreateVersion7();

        // Act
        var response = await client.DeleteResourceTypeAsync(unknownId);

        // Assert — resource type not found → ResourcesDomainException (CustomException) → 400
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // -----------------------------------------------------------------------------------------
    // DELETE /resources/types/{id} — cascade soft-deletes instances (T074)
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task DeleteResourceType_CascadesToInstances()
    {
        // Arrange — create a type with 3 instances
        var owner = await _users.CreateUserAsync("delrt_cascade_owner@test.com");
        var group = await _groups.CreateGroupReadModelAsync(owner.UserId);
        var client = new ResourcesTestClient(Factory, owner);

        var typeId = await client.CreateResourceTypeAndGetIdAsync(group.Id, "Type With Instances");
        var instanceId1 = await client.CreateResourceInstanceAndGetIdAsync(typeId, "Instance A");
        var instanceId2 = await client.CreateResourceInstanceAndGetIdAsync(typeId, "Instance B");
        var instanceId3 = await client.CreateResourceInstanceAndGetIdAsync(typeId, "Instance C");

        // Act — delete the type; cascade should soft-delete all instances
        var response = await client.DeleteResourceTypeAsync(typeId);

        // Assert — 204 No Content
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Assert — type is hard-deleted (NOT found even with IgnoreQueryFilters)
        var resourceType = await client.GetResourceTypeFromDbAsync(typeId);
        Assert.Null(resourceType);

        // Assert — GET /resources/types/{id} returns 404
        var getTypeResponse = await client.GetResourceTypeAsync(typeId);
        Assert.Equal(HttpStatusCode.NotFound, getTypeResponse.StatusCode);

        // Assert — all 3 instances are soft-deleted (DeletedAt set)
        var instance1 = await client.GetResourceInstanceFromDbAsync(instanceId1);
        var instance2 = await client.GetResourceInstanceFromDbAsync(instanceId2);
        var instance3 = await client.GetResourceInstanceFromDbAsync(instanceId3);

        Assert.NotNull(instance1);
        Assert.NotNull(instance1.DeletedAt);

        Assert.NotNull(instance2);
        Assert.NotNull(instance2.DeletedAt);

        Assert.NotNull(instance3);
        Assert.NotNull(instance3.DeletedAt);

        // Assert — instances are excluded from the default (non-deleted) list
        var listResponse = await client.GetResourceInstancesAsync(groupId: group.Id);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var pagedBody = await listResponse.Content.ReadFromJsonAsync<PagedInstancesResponse>(JsonOptions);
        Assert.NotNull(pagedBody);
        Assert.Empty(pagedBody.Items);
    }
}
