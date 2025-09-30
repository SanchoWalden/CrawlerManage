namespace CrawlerApi.Models.Dtos.Auth;

public record AuthenticatedUserDto(
    string Id,
    string? UserName,
    string? Email,
    string? DisplayName,
    IReadOnlyCollection<string> Roles
);

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    AuthenticatedUserDto User
);
