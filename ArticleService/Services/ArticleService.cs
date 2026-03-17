using ArticleService.DbContexts;
using ArticleService.Models;
using ArticleService.Repositories.Interfaces;
using ArticleService.Services.Interfaces;
using Events;
using Wolverine.EntityFrameworkCore;

namespace ArticleService.Services;

public class ArticleService(
	IArticleRepository repository,
	IDbContextOutbox<ArticleDbContext> outbox,
	IUnitOfWork unitOfWork) : IArticleService
{
	public async Task<IEnumerable<Article>> GetArticlesAsync() => await repository.GetAllWithCommentsAsync();

	public async Task<Article?> GetArticleByIdAsync(Guid id) => await repository.GetByIdWithCommentsAsync(id);

	public async Task<Article> CreateArticleAsync(CreateArticleRequest request, Guid userId)
	{
		var article = new Article
		{
			Id = Guid.NewGuid(),
			Title = request.Title,
			Content = request.Content,
			AuthorId = userId,
			CreatedAt = DateTime.UtcNow
		};

		repository.Add(article);
		await unitOfWork.CommitAsync();

		return article;
	}

	public async Task<Article?> UpdateArticleAsync(Guid id, UpdateArticleRequest request, Guid userId)
	{
		var article = await repository.FindByIdAndOwnerAsync(id, userId);

		if (article is null)
			return null;

		article.Title = request.Title;
		article.Content = request.Content;
		article.UpdatedAt = DateTime.UtcNow;

		await unitOfWork.CommitAsync();

		return article;
	}

	public async Task<bool> DeleteArticleAsync(Guid id, Guid userId)
	{
		var article = await repository.FindByIdAndOwnerAsync(id, userId);

		if (article is null)
			return false;

		repository.Remove(article);
		await unitOfWork.CommitAsync();

		return true;
	}

	public async Task<IEnumerable<Comment>> GetCommentsAsync(Guid articleId) => await repository.GetCommentsByArticleIdAsync(articleId);

	public async Task<Comment?> AddCommentAsync(Guid articleId, AddCommentRequest request, Guid userId)
	{
		var article = await repository.FindByIdAsync(articleId);

		if (article is null)
			return null;

		var comment = new Comment
		{
			Id = Guid.NewGuid(),
			Content = request.Content,
			AuthorId = userId,
			ArticleId = articleId,
			CreatedAt = DateTime.UtcNow
		};

		outbox.DbContext.Comments.Add(comment);
		await outbox.PublishAsync(new CommentAddedEvent(articleId, comment.Id, userId, article.Title, comment.Content));
		await unitOfWork.CommitAsync();

		return comment;
	}

	public async Task<bool> DeleteCommentAsync(Guid articleId, Guid commentId)
	{
		var comment = await repository.FindCommentAsync(articleId, commentId);

		if (comment is null)
			return false;

		var article = await repository.FindByIdAsync(articleId);

		outbox.DbContext.Comments.Remove(comment);
		await outbox.PublishAsync(new CommentDeletedEvent(articleId, commentId, comment.AuthorId, article!.Title, comment.Content));
		await unitOfWork.CommitAsync();

		return true;
	}
}
