using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models;

public record RevokeTokenRequest(
    [Required, EmailAddress] string Email);
