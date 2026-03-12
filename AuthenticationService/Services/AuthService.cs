using AuthenticationService.DbContexts;
using AuthenticationService.Models;
using AuthenticationService.Services.Interfaces;
using Events;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Wolverine.EntityFrameworkCore;

namespace AuthenticationService.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IConfiguration configuration,
    IDbContextOutbox<ApplicationDbContext> outbox,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        await using var transaction = await outbox.DbContext.Database.BeginTransactionAsync();

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return new AuthResponse(false, Errors: result.Errors.Select(e => e.Description));
        }

        var token = await tokenService.GenerateTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(GetRefreshTokenExpirationMinutes());
        await userManager.UpdateAsync(user);

        await outbox.PublishAsync(new UserCreatedEvent(request.Email));
        await unitOfWork.CommitAsync();

        return new AuthResponse(true, Token: token, RefreshToken: refreshToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return new AuthResponse(false, Errors: ["Invalid email or password."]);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return new AuthResponse(false, Errors: ["Invalid email or password."]);
        }

        var token = await tokenService.GenerateTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(GetRefreshTokenExpirationMinutes());
        await userManager.UpdateAsync(user);

        return new AuthResponse(true, Token: token, RefreshToken: refreshToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userId is null)
        {
            return new AuthResponse(false, Errors: ["Invalid token."]);
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResponse(false, Errors: ["Invalid or expired refresh token."]);
        }

        var newToken = await tokenService.GenerateTokenAsync(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(GetRefreshTokenExpirationMinutes());
        await userManager.UpdateAsync(user);

        return new AuthResponse(true, Token: newToken, RefreshToken: newRefreshToken);
    }

    public async Task<AuthResponse> RevokeTokenAsync(RevokeTokenRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return new AuthResponse(false, Errors: ["User not found."]);
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await userManager.UpdateAsync(user);

        return new AuthResponse(true);
    }

	public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
	{
		var user = await userManager.FindByEmailAsync(request.Email);

		if (user is null)
		{
			return new AuthResponse(true);
		}

		var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

		await outbox.PublishAsync(new PasswordResetRequestedEvent(user.Email!, resetToken));
		await unitOfWork.CommitAsync();

		return new AuthResponse(true);
	}

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return new AuthResponse(false, Errors: ["Invalid request."]);
        }

        var result = await userManager.ResetPasswordAsync(user, request.ResetToken, request.NewPassword);

        if (!result.Succeeded)
        {
            return new AuthResponse(false, Errors: result.Errors.Select(e => e.Description));
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await userManager.UpdateAsync(user);

        return new AuthResponse(true);
    }

    private double GetRefreshTokenExpirationMinutes() =>
        double.Parse(configuration["JwtSettings:RefreshTokenExpirationInMinutes"] ?? "15");
}
