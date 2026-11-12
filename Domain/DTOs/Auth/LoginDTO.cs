using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Auth;

public sealed class LoginDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
