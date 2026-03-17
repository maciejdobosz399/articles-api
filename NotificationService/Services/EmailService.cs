using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Settings;

namespace NotificationService.Services;

public class EmailService(IOptionsMonitor<EmailSettings> optionsMonitor, ILogger<EmailService> logger) : IEmailService
{
	private static readonly TimeSpan SmtpTimeout = TimeSpan.FromSeconds(30);

	public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
	{
		var settings = optionsMonitor.CurrentValue;

		var message = new MimeMessage();
		message.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
		message.To.Add(MailboxAddress.Parse(to));
		message.Subject = subject;
		message.Body = new TextPart("html") { Text = body };

		using var client = new SmtpClient();
		client.Timeout = (int)SmtpTimeout.TotalMilliseconds;

		if (settings.AllowInvalidCertificates)
			client.ServerCertificateValidationCallback = (_, _, _, _) => true;

		var tlsOptions = settings.UseTls ? SecureSocketOptions.StartTls : SecureSocketOptions.StartTlsWhenAvailable;
		await client.ConnectAsync(settings.Host, settings.Port, tlsOptions, cancellationToken);
		await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
		await client.SendAsync(message, cancellationToken);
		await client.DisconnectAsync(true, cancellationToken);

		logger.LogInformation("Email sent to {Recipient} with subject '{Subject}'", to, subject);
	}
}
