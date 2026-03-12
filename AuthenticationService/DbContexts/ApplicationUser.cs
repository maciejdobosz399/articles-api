using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.DbContexts;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
