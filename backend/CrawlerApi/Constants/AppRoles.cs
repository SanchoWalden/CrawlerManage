namespace CrawlerApi.Constants;

public static class AppRoles
{
    public const string Administrator = "Admin";
    public const string User = "User";

    public static IReadOnlyList<string> All { get; } = new[] { Administrator, User };
}
