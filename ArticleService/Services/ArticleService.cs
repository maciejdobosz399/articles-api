using ArticleService.DbContexts;
using ArticleService.Models;
using ArticleService.Services.Interfaces;
using Events;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace ArticleService.Services;

public class ArticleService(
	ArticleDbContext dbContext,
	IDbContextOutbox<ArticleDbContext> outbox,
	IUnitOfWork unitOfWork) : IArticleService
{
	public async Task<IEnumerable<Article>> GetArticlesAsync()
	{
		return await dbContext.Articles
			.AsNoTracking()
			.OrderByDescending(a => a.CreatedAt)
			.ToListAsync();
	}

	public async Task<Article?> GetArticleByIdAsync(Guid id)
	{
		return await dbContext.Articles
			.AsNoTracking()
			.Include(a => a.Comments)
			.FirstOrDefaultAsync(a => a.Id == id);
	}

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

		dbContext.Articles.Add(article);
		await unitOfWork.CommitAsync();

		return article;
	}

	public async Task<Article?> UpdateArticleAsync(Guid id, UpdateArticleRequest request)
	{
		var article = await dbContext.Articles.FindAsync(id);

		if (article is null)
			return null;

		article.Title = request.Title;
		article.Content = request.Content;
		article.UpdatedAt = DateTime.UtcNow;

		await unitOfWork.CommitAsync();

		return article;
	}

	public async Task<bool> DeleteArticleAsync(Guid id)
	{
		var article = await dbContext.Articles.FindAsync(id);

		if (article is null)
			return false;

		dbContext.Articles.Remove(article);
		await unitOfWork.CommitAsync();

		return true;
	}

	public async Task<IEnumerable<Comment>> GetCommentsAsync(Guid articleId)
	{
		return await dbContext.Comments
			.AsNoTracking()
			.Where(c => c.ArticleId == articleId)
			.OrderByDescending(c => c.CreatedAt)
			.ToListAsync();
	}

	public async Task<Comment?> AddCommentAsync(Guid articleId, AddCommentRequest request, Guid userId)
	{
		var article = await dbContext.Articles.FindAsync(articleId);

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
		await outbox.PublishAsync(new CommentAddedEvent(articleId, comment.Id, userId));
		await unitOfWork.CommitAsync();

		return comment;
	}

	public async Task<bool> DeleteCommentAsync(Guid articleId, Guid commentId)
	{
		var comment = await dbContext.Comments
			.FirstOrDefaultAsync(c => c.Id == commentId && c.ArticleId == articleId);

		if (comment is null)
			return false;

		outbox.DbContext.Comments.Remove(comment);
		await outbox.PublishAsync(new CommentDeletedEvent(articleId, commentId, comment.AuthorId));
		await unitOfWork.CommitAsync();

		return true;
	}
}
