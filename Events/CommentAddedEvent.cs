namespace Events;

public record CommentAddedEvent(Guid ArticleId, Guid CommentId, Guid AuthorId, string CommentAuthorEmail, string ArticleAuthorEmail, string ArticleTitle, string CommentContent) : Event;
