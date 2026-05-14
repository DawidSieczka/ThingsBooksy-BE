using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Shared.IntegrationTests;
using ThingsBooksy.Shared.IntegrationTests.Clients;
using Xunit;

namespace ThingsBooksy.Modules.Users.IntegrationTests;

[Collection("IntegrationTestCollection")]
public class LogoutEndpointTests : IntegrationTestBase
{
    private readonly UsersTestClient _users;

    public LogoutEndpointTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new UsersTestClient(factory);
    }

    [Fact]
    public async Task Logout_WithValidToken_Returns200_AndPersistsRevocation()
    {
        var user = await _users.RegisterAndLoginAsync("logout_valid@test.com");

        var response = await user.Client.PostAsync("/users/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var revocations = await db.TokenRevocations
            .IgnoreQueryFilters()
            .Where(x => x.UserId == user.UserId)
            .ToListAsync();

        Assert.Single(revocations);
    }

    [Fact]
    public async Task GetMe_AfterLogout_WithSameToken_Returns401()
    {
        var user = await _users.RegisterAndLoginAsync("logout_getme@test.com");

        var logoutResponse = await user.Client.PostAsync("/users/logout", null);
        logoutResponse.EnsureSuccessStatusCode();

        var getMeResponse = await user.Client.GetAsync("/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, getMeResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_TwiceWithSameToken_FirstReturns200_SecondReturns401()
    {
        var user = await _users.RegisterAndLoginAsync("logout_twice@test.com");

        var firstResponse = await user.Client.PostAsync("/users/logout", null);
        var secondResponse = await user.Client.PostAsync("/users/logout", null);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutToken_Returns401()
    {
        var anonymousClient = Factory.CreateClient();

        var response = await anonymousClient.PostAsync("/users/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
