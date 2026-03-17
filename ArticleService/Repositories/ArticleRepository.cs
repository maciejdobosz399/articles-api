using ArticleService.DbContexts;
using ArticleService.Models;
using ArticleService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Repositories;

public class ArticleRepository(ArticleDbContext dbContext) : IArticleRepository
{
	public async Task<IEnumerable<Article>> GetAllWithCommentsAsync()
	{
		return await dbContext.Articles
			.AsNoTracking()
			.Include(a => a.Comments)
			.OrderByDescending(a => a.CreatedAt)
			.ToListAsync();
	}

	public async Task<Article?> GetByIdWithCommentsAsync(Guid id)
	{
		return await dbContext.Articles
			.AsNoTracking()
			.Include(a => a.Comments)
			.FirstOrDefaultAsync(a => a.Id == id);
	}

	public async Task<Article?> FindByIdAndOwnerAsync(Guid id, Guid userId)
	{
		return await dbContext.Articles
			.FirstOrDefaultAsync(a => a.Id == id && a.AuthorId == userId);
	}

	public async Task<Article?> FindByIdAsync(Guid id) => await dbContext.Articles.FindAsync(id);

	public void Add(Article article) => dbContext.Articles.Add(article);

	public void Remove(Article article) => dbContext.Articles.Remove(article);

	public async Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(Guid articleId)
	{
		return await dbContext.Comments
			.AsNoTracking()
			.Where(c => c.ArticleId == articleId)
			.OrderByDescending(c => c.CreatedAt)
			.ToListAsync();
	}

	public async Task<Comment?> FindCommentAsync(Guid articleId, Guid commentId)
	{
		return await dbContext.Comments
			.FirstOrDefaultAsync(c => c.Id == commentId && c.ArticleId == articleId);
	}
}