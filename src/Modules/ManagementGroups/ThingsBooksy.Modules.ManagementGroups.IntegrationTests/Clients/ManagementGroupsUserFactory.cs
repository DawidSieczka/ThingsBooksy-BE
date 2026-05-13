using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;
using ThingsBooksy.Shared.Abstractions.Events.Users;
using ThingsBooksy.Shared.Infrastructure.Auth;
using ThingsBooksy.Shared.IntegrationTests;
using ThingsBooksy.Shared.IntegrationTests.Clients;

namespace ThingsBooksy.Modules.ManagementGroups.IntegrationTests.Clients;

/// <summary>
/// Creates authenticated users for ManagementGroups integration tests by inserting
/// a UserReadModel directly into the DB and generating a valid JWT — no dependency on Users module.
/// </summary>
public sealed class ManagementGroupsUserFactory
{
    private readonly ThingsBooksyWebAppFactory _factory;

    public ManagementGroupsUserFactory(ThingsBooksyWebAppFactory factory)
    {
        _factory = factory;
    }

    public async Task<AuthenticatedUser> CreateUserAsync(string email)
    {
        var userId = Guid.CreateVersion7();

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ManagementGroupsDbContext>();
        db.UserReadModels.Add(UserReadModel.Upsert(new UserSignedUp(userId, email)));
        await db.SaveChangesAsync();

        var authOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Jwt.IssuerSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Aud, authOptions.Jwt.Audience),
            new(ClaimTypes.Role, "user"),
        };

        var token = new JwtSecurityToken(
            issuer: authOptions.Jwt.Issuer,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

        return new AuthenticatedUser(client, userId, email);
    }
}
