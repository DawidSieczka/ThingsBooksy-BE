using Microsoft.AspNetCore.Http;
using ThingsBooksy.Shared.Abstractions.Auth;

namespace ThingsBooksy.Modules.Users.Core.Services;

internal sealed class HttpContextTokenStorage : ITokenStorage
{
    private const string TokenKey = "jwt";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTokenStorage(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public void Set(JsonWebToken jwt)
        => _httpContextAccessor.HttpContext?.Items.TryAdd(TokenKey, jwt);

    public JsonWebToken? Get()
    {
        if (_httpContextAccessor.HttpContext is null)
            return null;

        return _httpContextAccessor.HttpContext.Items.TryGetValue(TokenKey, out var jwt)
            ? jwt as JsonWebToken
            : null;
    }
}
