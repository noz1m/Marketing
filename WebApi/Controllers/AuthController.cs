using System.Net;
using Domain.ApiResponse;
using Domain.DTOs.Auth;
using Infrastructure.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    private static string FormatErrors(IdentityResult result)
        => string.Join("; ", result.Errors.Select(error => error.Description));

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDTO dto)
    {
        var result = await service.RegisterAsync(dto);
        return result.Succeeded
            ? Ok(Response<string>.Success("Registered", "Registration successful."))
            : BadRequest(Response<string>.Fail(FormatErrors(result), HttpStatusCode.BadRequest));
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDTO dto)
    {
        var token = await service.LoginAsync(dto);
        return token is not null
            ? Ok(new { Token = token })
            : Unauthorized(Response<string>.Fail("Invalid credentials or locked account.", HttpStatusCode.Unauthorized));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] PasswordChangeDTO dto)
    {
        var result = await service.ChangePasswordAsync(dto);
        return result.Succeeded
            ? Ok(Response<string>.Success("Changed", "Password changed successfully."))
            : BadRequest(Response<string>.Fail(FormatErrors(result), HttpStatusCode.BadRequest));
    }

    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordResetAsync(
        [FromQuery] string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(Response<string>.Fail("Email is required.", HttpStatusCode.BadRequest));

        await service.RequestPasswordResetAsync(email, cancellationToken);
        return Ok(Response<string>.Success(
            "Requested", "If the account exists, a reset email has been sent."));
    }

    [HttpPost("password-reset")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetDTO dto)
    {
        var result = await service.ResetAsync(dto);
        return result.Succeeded
            ? Ok(Response<string>.Success("Reset", "Password reset successfully."))
            : BadRequest(Response<string>.Fail("Invalid email, token or password.", HttpStatusCode.BadRequest));
    }
}
