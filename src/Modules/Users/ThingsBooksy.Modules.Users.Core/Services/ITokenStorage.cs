using ThingsBooksy.Shared.Abstractions.Auth;

namespace ThingsBooksy.Modules.Users.Core.Services;

internal interface ITokenStorage
{
    void Set(JsonWebToken jwt);
    JsonWebToken Get();
}
