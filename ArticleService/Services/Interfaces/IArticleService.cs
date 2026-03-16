using ArticleService.Models;

namespace ArticleService.Services.Interfaces;

public interface IArticleService
{
	Task<IEnumerable<Article>> GetArticlesAsync();
	Task<Article?> GetArticleByIdAsync(Guid id);
	Task<Article> CreateArticleAsync(CreateArticleRequest request);
	Task<Article?> UpdateArticleAsync(Guid id, UpdateArticleRequest request);
	Task<bool> DeleteArticleAsync(Guid id);
	Task<IEnumerable<Comment>> GetCommentsAsync(Guid articleId);
	Task<Comment?> AddCommentAsync(Guid articleId, AddCommentRequest request);
	Task<bool> DeleteCommentAsync(Guid articleId, Guid commentId);
}
