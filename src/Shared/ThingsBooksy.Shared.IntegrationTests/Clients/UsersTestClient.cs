using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.Auth;

namespace ThingsBooksy.Shared.IntegrationTests.Clients;

public record AuthenticatedUser(HttpClient Client, Guid UserId, string Email);

public class UsersTestClient
{
    private readonly ThingsBooksyWebAppFactory _factory;

    public UsersTestClient(ThingsBooksyWebAppFactory factory)
    {
        _factory = factory;
    }

    public Task<HttpResponseMessage> SignUpAsync(string email, string password = "Test1234!")
        => _factory.CreateClient().PostAsJsonAsync("/users/sign-up", new { Email = email, Password = password });

    public Task<HttpResponseMessage> SignInAsync(string email, string password = "Test1234!")
        => _factory.CreateClient().PostAsJsonAsync("/users/sign-in", new { Email = email, Password = password });

    public async Task<AuthenticatedUser> RegisterAndLoginAsync(string email, string password = "Test1234!")
    {
        (await SignUpAsync(email, password)).EnsureSuccessStatusCode();

        var signInResponse = await SignInAsync(email, password);
        signInResponse.EnsureSuccessStatusCode();

        var jwt = await signInResponse.Content.ReadFromJsonAsync<JsonWebToken>();
        ArgumentNullException.ThrowIfNull(jwt);

        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt.AccessToken);

        return new AuthenticatedUser(authenticatedClient, Guid.Parse(jwt.UserId), email);
    }
}
