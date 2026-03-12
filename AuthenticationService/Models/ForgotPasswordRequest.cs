using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models;

public record ForgotPasswordRequest(
    [Required, EmailAddress] string Email);
