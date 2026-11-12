using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Auth;

public sealed class PasswordChangeDTO
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
