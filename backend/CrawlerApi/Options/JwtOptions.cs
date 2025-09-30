namespace CrawlerApi.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SchoolManage";
    public string Audience { get; set; } = "SchoolManageClient";
    public string Secret { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 120;
}
