using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;
using ThingsBooksy.Shared.IntegrationTests;
using ThingsBooksy.Shared.IntegrationTests.Clients;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;

public record CreateGroupResponse(Guid Id);

public class ManagementGroupsTestClient
{
    private readonly HttpClient _client;
    private readonly ThingsBooksyWebAppFactory _factory;

    public ManagementGroupsTestClient(ThingsBooksyWebAppFactory factory, AuthenticatedUser user)
    {
        _factory = factory;
        _client = user.Client;
    }

    // --- HTTP methods (Arrange / Act) ---

    public Task<HttpResponseMessage> CreateGroupAsync(string name, string? description = null)
        => _client.PostAsJsonAsync("/management-groups", new { Name = name, Description = description });

    public async Task<Guid> CreateGroupAndGetIdAsync(string name, string? description = null)
    {
        var response = await CreateGroupAsync(name, description);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateGroupResponse>();
        return result!.Id;
    }

    public Task<HttpResponseMessage> GetGroupsAsync()
        => _client.GetAsync("/management-groups");

    public Task<HttpResponseMessage> GetGroupAsync(Guid id)
        => _client.GetAsync($"/management-groups/{id}");

    public Task<HttpResponseMessage> UpdateGroupAsync(Guid id, string name, string? description = null)
        => _client.PutAsJsonAsync($"/management-groups/{id}", new { Name = name, Description = description });

    public Task<HttpResponseMessage> DeleteGroupAsync(Guid id)
        => _client.DeleteAsync($"/management-groups/{id}");

    public Task<HttpResponseMessage> RestoreGroupAsync(Guid id)
        => _client.PostAsJsonAsync($"/management-groups/{id}/restore", new { });

    public Task<HttpResponseMessage> AddMemberAsync(Guid groupId, string email)
        => _client.PostAsJsonAsync($"/management-groups/{groupId}/members", new { Email = email });

    public Task<HttpResponseMessage> RemoveMemberAsync(Guid groupId, Guid userId)
        => _client.DeleteAsync($"/management-groups/{groupId}/members/{userId}");

    // --- DB methods (Assert) ---

    internal async Task<ManagementGroup?> GetGroupFromDbAsync(Guid id)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ManagementGroupsDbContext>();
        return await db.ManagementGroups
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    internal async Task<List<GroupMember>> GetMembersFromDbAsync(Guid groupId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ManagementGroupsDbContext>();
        return await db.GroupMembers
            .Where(x => x.GroupId == groupId)
            .ToListAsync();
    }

    internal async Task<UserReadModel?> GetUserReadModelFromDbAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ManagementGroupsDbContext>();
        return await db.UserReadModels
            .FirstOrDefaultAsync(x => x.Email == email);
    }
}
