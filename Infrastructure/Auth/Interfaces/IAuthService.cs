using Domain.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Auth.Interfaces;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterDTO dto);
    Task<string?> LoginAsync(LoginDTO dto);
    Task<IdentityResult> ChangePasswordAsync(PasswordChangeDTO dto);
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<IdentityResult> ResetAsync(ResetDTO dto);
}
