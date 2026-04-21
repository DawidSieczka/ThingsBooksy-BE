using ThingsBooksy.Shared.Abstractions.Auth;

namespace ThingsBooksy.Modules.Users.Core.Services;

public interface ITokenStorage
{
    void Set(JsonWebToken jwt);
    JsonWebToken Get();
}
