namespace ArticleService.Models;

public record AddCommentRequest(string Content, Guid AuthorId);
