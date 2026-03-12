using AuthenticationService.DbContexts;
using AuthenticationService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationService.Services;

public class TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager) : ITokenService
{
	public async Task<string> GenerateTokenAsync(IdentityUser<Guid> user)
	{
		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var roles = await userManager.GetRolesAsync((ApplicationUser)user);

		foreach (var role in roles)
		{
			claims.Add(new Claim("role", role));
		}

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: configuration["JwtSettings:Issuer"],
			audience: configuration["JwtSettings:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(double.Parse(configuration["JwtSettings:ExpirationInMinutes"] ?? "60")),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		var randomNumber = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}

	public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!));

		var tokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = false,
			ValidateIssuerSigningKey = true,
			ValidIssuer = configuration["JwtSettings:Issuer"],
			ValidAudience = configuration["JwtSettings:Audience"],
			IssuerSigningKey = key
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

		if (securityToken is not JwtSecurityToken jwtSecurityToken ||
			!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
		{
			throw new SecurityTokenException("Invalid token.");
		}

		return principal;
	}
}
