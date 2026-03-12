using Asp.Versioning;
using AuthenticationService.Models;
using AuthenticationService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthenticationController(IAuthService authenticationService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await authenticationService.RegisterAsync(request);
        return response.Succeeded ? Ok(response) : BadRequest(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authenticationService.LoginAsync(request);
        return response.Succeeded ? Ok(response) : Unauthorized(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await authenticationService.RefreshTokenAsync(request);
        return response.Succeeded ? Ok(response) : Unauthorized(response);
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var response = await authenticationService.RevokeTokenAsync(request);
        return response.Succeeded ? Ok(response) : BadRequest(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var response = await authenticationService.ForgotPasswordAsync(request);
        return Ok(response);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var response = await authenticationService.ResetPasswordAsync(request);
        return response.Succeeded ? Ok(response) : BadRequest(response);
    }
}
