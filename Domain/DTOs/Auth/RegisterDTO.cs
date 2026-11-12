using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Auth;

public sealed class RegisterDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
