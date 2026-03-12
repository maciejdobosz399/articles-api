using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models;

public record LoginRequest(
	[Required, EmailAddress] string Email,
	[Required] string Password);
