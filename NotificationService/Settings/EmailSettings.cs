using System.ComponentModel.DataAnnotations;

namespace NotificationService.Settings;

public class EmailSettings
{
	[Required]
	public required string Host { get; set; }

	[Range(1, 65535)]
	public int Port { get; set; } = 587;

	[Required, EmailAddress]
	public required string SenderEmail { get; set; }

	[Required]
	public required string SenderName { get; set; }

	[Required]
	public required string Username { get; set; }

	[Required]
	public required string Password { get; set; }

	public bool AllowInvalidCertificates { get; set; }

	public bool UseTls { get; set; } = true;
}
