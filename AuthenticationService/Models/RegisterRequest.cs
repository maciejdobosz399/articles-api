using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models;

public record RegisterRequest(
	[Required, EmailAddress] string Email,
	[Required, MinLength(8)] string Password);
