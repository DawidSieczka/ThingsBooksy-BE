using System.Net;
using System.Threading.Tasks;
using ThingsBooksy.Shared.IntegrationTests;
using ThingsBooksy.Shared.IntegrationTests.Clients;
using Xunit;

namespace ThingsBooksy.Modules.Users.IntegrationTests;

public class SignInEndpointTests : IntegrationTestBase
{
    private readonly UsersTestClient _users;

    public SignInEndpointTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new UsersTestClient(factory);
    }

    [Fact]
    public async Task SignIn_WithValidCredentials_Returns200WithToken()
    {
        await _users.SignUpAsync("signin_valid@test.com");

        var response = await _users.SignInAsync("signin_valid@test.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", body, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SignIn_WithInvalidPassword_Returns400()
    {
        await _users.SignUpAsync("signin_badpass@test.com");

        var response = await _users.SignInAsync("signin_badpass@test.com", "WrongPassword!");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SignIn_WithNonExistentEmail_Returns400()
    {
        var response = await _users.SignInAsync("does_not_exist@test.com");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
