namespace ThingsBooksy.Modules.Users.Core.Features.GetUser;

internal record GetUserQueryResult(
    Guid UserId,
    string Email,
    string Role,
    string? JobTitle,
    DateTime CreatedAt,
    IEnumerable<string> Permissions);
