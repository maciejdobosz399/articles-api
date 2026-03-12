using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Models;

public record ResetPasswordRequest(
    [Required, EmailAddress] string Email,
    [Required] string ResetToken,
    [Required, MinLength(8)] string NewPassword);
