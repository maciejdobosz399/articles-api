using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Services.Interfaces;

public interface ITokenService
{
	Task<string> GenerateTokenAsync(IdentityUser<Guid> user);

	string GenerateRefreshToken();

	ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}