namespace Events;

public record CommentAddedEvent(Guid ArticleId, Guid CommentId, Guid AuthorId) : Event;
