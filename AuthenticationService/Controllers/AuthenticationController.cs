using Asp.Versioning;
using AuthenticationService.Models;
using AuthenticationService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;

/// <summary>
/// Handles user authentication, token management, and password operations.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
public class AuthenticationController(IAuthService authenticationService) : ControllerBase
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration details including email and password.</param>
    /// <returns>The authentication response indicating success or failure.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Registration failed due to validation errors.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await authenticationService.RegisterAsync(request);
        return response.Succeeded ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="request">The login credentials including email and password.</param>
    /// <returns>The authentication response containing tokens on success.</returns>
    /// <response code="200">Login successful. Returns access and refresh tokens.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authenticationService.LoginAsync(request);
        return response.Succeeded ? Ok(response) : Unauthorized(response);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The current access token and refresh token pair.</param>
    /// <returns>A new access and refresh token pair on success.</returns>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="401">Invalid or expired tokens.</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await authenticationService.RefreshTokenAsync(request);
        return response.Succeeded ? Ok(response) : Unauthorized(response);
    }

    /// <summary>
    /// Revokes the refresh token for the specified user.
    /// </summary>
    /// <param name="request">The email of the user whose token should be revoked.</param>
    /// <returns>The result of the revocation operation.</returns>
    /// <response code="200">Token revoked successfully.</response>
    /// <response code="400">Revocation failed.</response>
    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var response = await authenticationService.RevokeTokenAsync(request);
        return response.Succeeded ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Initiates a password reset flow by sending a reset link to the user's email.
    /// </summary>
    /// <param name="request">The email address of the user requesting a password reset.</param>
    /// <returns>A success response regardless of whether the email exists, to prevent enumeration.</returns>
    /// <response code="200">Password reset email sent (if the account exists).</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var response = await authenticationService.ForgotPasswordAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Resets the user's password using a valid reset token.
    /// </summary>
    /// <param name="request">The reset details including email, reset token, and new password.</param>
    /// <returns>The result of the password reset operation.</returns>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Password reset failed due to an invalid token or validation errors.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var response = await authenticationService.ResetPasswordAsync(request);
        return response.Succeeded ? Ok(response) : BadRequest(response);
    }
}
