using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models;

public record RefreshTokenRequest(
    [Required] string AccessToken,
    [Required] string RefreshToken);
