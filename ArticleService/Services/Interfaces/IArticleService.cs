using ArticleService.Models;

namespace ArticleService.Services.Interfaces;

public interface IArticleService
{
	Task<IEnumerable<Article>> GetArticlesAsync();

	Task<Article?> GetArticleByIdAsync(Guid id);

	Task<Article> CreateArticleAsync(CreateArticleRequest request, Guid userId);

	Task<Article?> UpdateArticleAsync(Guid id, UpdateArticleRequest request, Guid userId);

	Task<bool> DeleteArticleAsync(Guid id, Guid userId);

	Task<IEnumerable<Comment>> GetCommentsAsync(Guid articleId);

	Task<Comment?> AddCommentAsync(Guid articleId, AddCommentRequest request, Guid userId);

	Task<bool> DeleteCommentAsync(Guid articleId, Guid commentId);
}