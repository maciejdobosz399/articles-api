using Events;
using NotificationService.Services;

namespace NotificationService.Handlers;

public class UserCreatedEventHandler
{
	public static async Task Handle(UserCreatedEvent @event, IEmailService emailService, ILogger<UserCreatedEventHandler> logger)
	{
		var subject = "Welcome to Articles!";
		var body = $"""
			<h1>Welcome!</h1>
			<p>Your account has been created successfully.</p>
			<p>You can now start reading and commenting on articles.</p>
			""";

		await emailService.SendEmailAsync(@event.Email, subject, body);

		logger.LogInformation("Welcome email sent to {Email}", @event.Email);
	}
}
