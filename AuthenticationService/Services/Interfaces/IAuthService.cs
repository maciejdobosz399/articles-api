using AuthenticationService.Models;

namespace AuthenticationService.Services.Interfaces;

public interface IAuthService
{
	Task<AuthResponse> RegisterAsync(RegisterRequest request);

	Task<AuthResponse> LoginAsync(LoginRequest request);

	Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

	Task<AuthResponse> RevokeTokenAsync(RevokeTokenRequest request);

	Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);

	Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
}