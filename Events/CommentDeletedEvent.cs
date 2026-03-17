namespace Events;

public record CommentDeletedEvent(Guid ArticleId, Guid CommentId, Guid AuthorId) : Event;
