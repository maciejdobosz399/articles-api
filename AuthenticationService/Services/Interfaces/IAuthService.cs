using AuthenticationService.Models;

namespace AuthenticationService.Services.Interfaces;

/// <summary>
/// Provides authentication and account management operations.
/// </summary>
public interface IAuthService
{
	/// <summary>
	/// Registers a new user account.
	/// </summary>
	/// <param name="request">The registration request containing the user's credentials and details.</param>
	/// <returns>An <see cref="AuthResponse"/> indicating the result of the registration.</returns>
	Task<AuthResponse> RegisterAsync(RegisterRequest request);

	/// <summary>
	/// Authenticates a user with their credentials.
	/// </summary>
	/// <param name="request">The login request containing the user's credentials.</param>
	/// <returns>An <see cref="AuthResponse"/> containing authentication tokens on success.</returns>
	Task<AuthResponse> LoginAsync(LoginRequest request);

	/// <summary>
	/// Refreshes an expired authentication token using a valid refresh token.
	/// </summary>
	/// <param name="request">The request containing the refresh token.</param>
	/// <returns>An <see cref="AuthResponse"/> containing the new authentication tokens.</returns>
	Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

	/// <summary>
	/// Revokes an active refresh token, preventing further use.
	/// </summary>
	/// <param name="request">The request containing the token to revoke.</param>
	/// <returns>An <see cref="AuthResponse"/> indicating the result of the revocation.</returns>
	Task<AuthResponse> RevokeTokenAsync(RevokeTokenRequest request);

	/// <summary>
	/// Initiates the forgot-password flow by sending a password reset link to the user's email.
	/// </summary>
	/// <param name="request">The request containing the user's email address.</param>
	/// <returns>An <see cref="AuthResponse"/> indicating the result of the operation.</returns>
	Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);

	/// <summary>
	/// Resets a user's password using a valid reset token.
	/// </summary>
	/// <param name="request">The request containing the reset token and new password.</param>
	/// <returns>An <see cref="AuthResponse"/> indicating the result of the password reset.</returns>
	Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
}