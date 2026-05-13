namespace ThingsBooksy.Modules.Users.Api.Requests;

public record SignUpRequest(string Email, string Password, string? JobTitle = null, string? Role = null);
