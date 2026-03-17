using ArticleService.Models;

namespace ArticleService.Repositories.Interfaces;

public interface IArticleRepository
{
	Task<IEnumerable<Article>> GetAllWithCommentsAsync();

	Task<Article?> GetByIdWithCommentsAsync(Guid id);

	Task<Article?> FindByIdAndOwnerAsync(Guid id, Guid userId);

	Task<Article?> FindByIdAsync(Guid id);

	void Add(Article article);

	void Remove(Article article);

	Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(Guid articleId);

	Task<Comment?> FindCommentAsync(Guid articleId, Guid commentId);
}