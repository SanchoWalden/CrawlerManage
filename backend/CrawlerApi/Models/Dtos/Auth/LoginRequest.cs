using System.ComponentModel.DataAnnotations;

namespace CrawlerApi.Models.Dtos.Auth;

public class LoginRequest
{
    [Required]
    public string EmailOrUserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
