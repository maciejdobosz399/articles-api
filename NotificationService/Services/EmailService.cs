using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Settings;

namespace NotificationService.Services;

public class EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger) : IEmailService
{
	private readonly EmailSettings _settings = options.Value;

	public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
	{
		var message = new MimeMessage();
		message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
		message.To.Add(MailboxAddress.Parse(to));
		message.Subject = subject;
		message.Body = new TextPart("html") { Text = body };

		using var client = new SmtpClient();

		await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
		await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
		await client.SendAsync(message, cancellationToken);
		await client.DisconnectAsync(true, cancellationToken);

		logger.LogInformation("Email sent to {Recipient} with subject '{Subject}'", to, subject);
	}
}
