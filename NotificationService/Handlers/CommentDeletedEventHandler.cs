using Events;

namespace NotificationService.Handlers;

public class CommentDeletedEventHandler
{
	public static void Handle(CommentDeletedEvent @event, ILogger<CommentDeletedEventHandler> logger)
	{
		logger.LogInformation(
			"Comment deleted on article '{ArticleTitle}' (ArticleId: {ArticleId}, CommentId: {CommentId}) by user {AuthorId}",
			@event.ArticleTitle, @event.ArticleId, @event.CommentId, @event.AuthorId);
	}
}
