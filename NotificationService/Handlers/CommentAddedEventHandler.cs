using Events;
using NotificationService.Services;

namespace NotificationService.Handlers;

public class CommentAddedEventHandler
{
	public static async Task Handle(CommentAddedEvent @event, IEmailService emailService, ILogger<CommentAddedEventHandler> logger)
	{
		var subject = $"New comment on your article \"{@event.ArticleTitle}\"";
		var body = $"""
			<h1>New Comment on Your Article</h1>
			<p>A new comment has been added to your article <strong>{@event.ArticleTitle}</strong>.</p>
			<blockquote>{@event.CommentContent}</blockquote>
			""";

		await emailService.SendEmailAsync(@event.ArticleAuthorEmail, subject, body);

		logger.LogInformation(
			"Comment added notification sent to {Email} for article '{ArticleTitle}' (CommentId: {CommentId})",
			@event.ArticleAuthorEmail, @event.ArticleTitle, @event.CommentId);
	}
}
