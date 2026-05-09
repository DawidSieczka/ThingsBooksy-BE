using System.Net;
using System.Threading.Tasks;
using ThingsBooksy.Shared.IntegrationTests;
using ThingsBooksy.Shared.IntegrationTests.Clients;
using Xunit;

namespace ThingsBooksy.Modules.Users.IntegrationTests;

public class SignUpEndpointTests : IntegrationTestBase
{
    private readonly UsersTestClient _users;

    public SignUpEndpointTests(ThingsBooksyWebAppFactory factory) : base(factory)
    {
        _users = new UsersTestClient(factory);
    }

    [Fact]
    public async Task SignUp_WithValidData_Returns204()
    {
        var response = await _users.SignUpAsync("signup_valid@test.com");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SignUp_WithDuplicateEmail_Returns400()
    {
        await _users.SignUpAsync("signup_dup@test.com");

        var response = await _users.SignUpAsync("signup_dup@test.com");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SignUp_WithoutPassword_Returns400()
    {
        var response = await _users.SignUpAsync("signup_nopass@test.com", "");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
