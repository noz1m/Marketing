using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.DTOs.Auth;
using Infrastructure.Auth.Interfaces;
using Infrastructure.Email.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth.Services;

public sealed class AuthService(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    IEmailService emailService) : IAuthService
{
    public async Task<IdentityResult> RegisterAsync(RegisterDTO dto)
    {
        var email = dto.Email.Trim();
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = dto.PhoneNumber
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return result;

        var roleResult = await userManager.AddToRoleAsync(user, "User");
        if (roleResult.Succeeded)
            return result;

        await userManager.DeleteAsync(user);
        return roleResult;
    }

    public async Task<string?> LoginAsync(LoginDTO dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email.Trim());
        if (user is null)
            return null;

        var signIn = await signInManager.CheckPasswordSignInAsync(
            user, dto.Password, lockoutOnFailure: true);

        return signIn.Succeeded ? await GenerateJwtTokenAsync(user) : null;
    }

    private async Task<string> GenerateJwtTokenAsync(IdentityUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<IdentityResult> ChangePasswordAsync(PasswordChangeDTO dto)
    {
        var principal = httpContextAccessor.HttpContext?.User;
        var user = principal is null ? null : await userManager.GetUserAsync(principal);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "Unauthorized." });

        return await userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
    }

    public async Task RequestPasswordResetAsync(
        string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
            return;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await emailService.SendPasswordResetAsync(user.Email!, token, cancellationToken);
    }

    public async Task<IdentityResult> ResetAsync(ResetDTO dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email.Trim());
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "Invalid email or token." });

        return await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
    }
}
