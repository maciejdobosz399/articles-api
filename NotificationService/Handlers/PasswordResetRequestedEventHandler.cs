using Events;
using NotificationService.Services;

namespace NotificationService.Handlers;

public class PasswordResetRequestedEventHandler
{
	public static async Task Handle(PasswordResetRequestedEvent @event, IEmailService emailService, ILogger<PasswordResetRequestedEventHandler> logger)
	{
		var subject = "Password Reset Request";
		var body = $"""
			<h1>Password Reset</h1>
			<p>We received a request to reset your password.</p>
			<p>Use the following token to reset your password:</p>
			<p><strong>{@event.ResetToken}</strong></p>
			<p>If you did not request a password reset, please ignore this email.</p>
			""";

		await emailService.SendEmailAsync(@event.Email, subject, body);

		logger.LogInformation("Password reset email sent to {Email}", @event.Email);
	}
}
