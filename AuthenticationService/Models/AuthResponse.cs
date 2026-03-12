namespace AuthenticationService.Models;

public record AuthResponse(bool Succeeded, string? Token = null, string? RefreshToken = null, IEnumerable<string>? Errors = null);
