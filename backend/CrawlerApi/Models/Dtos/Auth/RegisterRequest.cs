using System.ComponentModel.DataAnnotations;

namespace CrawlerApi.Models.Dtos.Auth;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? DisplayName { get; set; }
}
