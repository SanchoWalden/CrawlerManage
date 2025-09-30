using Microsoft.AspNetCore.Identity;

namespace CrawlerApi.Models;

public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
