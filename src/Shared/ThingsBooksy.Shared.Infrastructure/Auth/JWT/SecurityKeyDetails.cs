using Microsoft.IdentityModel.Tokens;

namespace ThingsBooksy.Shared.Infrastructure.Auth.JWT;

internal sealed record SecurityKeyDetails(SecurityKey Key, string Algorithm);
