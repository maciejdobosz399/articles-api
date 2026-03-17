namespace NotificationService.Settings;

public class EmailSettings
{
	public required string Host { get; set; }
	public int Port { get; set; } = 587;
	public required string SenderEmail { get; set; }
	public required string SenderName { get; set; }
	public required string Username { get; set; }
	public required string Password { get; set; }
}
