using ArticleService.Models;

namespace ArticleService.Services.Interfaces;

/// <summary>
/// Provides operations for managing articles and their associated comments.
/// </summary>
public interface IArticleService
{
	/// <summary>
	/// Retrieves all articles.
	/// </summary>
	/// <returns>A collection of all articles.</returns>
	Task<IEnumerable<Article>> GetArticlesAsync();

	/// <summary>
	/// Retrieves an article by its unique identifier.
	/// </summary>
	/// <param name="id">The unique identifier of the article.</param>
	/// <returns>The matching article, or <c>null</c> if not found.</returns>
	Task<Article?> GetArticleByIdAsync(Guid id);

	/// <summary>
	/// Creates a new article.
	/// </summary>
	/// <param name="request">The article creation request containing the article details.</param>
	/// <param name="userId">The unique identifier of the user creating the article.</param>
	/// <param name="userEmail">The email address of the user creating the article.</param>
	/// <returns>The newly created article.</returns>
	Task<Article> CreateArticleAsync(CreateArticleRequest request, Guid userId, string userEmail);

	/// <summary>
	/// Updates an existing article.
	/// </summary>
	/// <param name="id">The unique identifier of the article to update.</param>
	/// <param name="request">The update request containing the modified article details.</param>
	/// <param name="userId">The unique identifier of the user performing the update.</param>
	/// <returns>The updated article, or <c>null</c> if the article was not found.</returns>
	Task<Article?> UpdateArticleAsync(Guid id, UpdateArticleRequest request, Guid userId);

	/// <summary>
	/// Deletes an article.
	/// </summary>
	/// <param name="id">The unique identifier of the article to delete.</param>
	/// <param name="userId">The unique identifier of the user performing the deletion.</param>
	/// <returns><c>true</c> if the article was successfully deleted; otherwise, <c>false</c>.</returns>
	Task<bool> DeleteArticleAsync(Guid id, Guid userId);

	/// <summary>
	/// Retrieves all comments for a specific article.
	/// </summary>
	/// <param name="articleId">The unique identifier of the article.</param>
	/// <returns>A collection of comments belonging to the specified article.</returns>
	Task<IEnumerable<Comment>> GetCommentsAsync(Guid articleId);

	/// <summary>
	/// Adds a comment to an article.
	/// </summary>
	/// <param name="articleId">The unique identifier of the article to comment on.</param>
	/// <param name="request">The comment request containing the comment details.</param>
	/// <param name="userId">The unique identifier of the user adding the comment.</param>
	/// <param name="userEmail">The email address of the user adding the comment.</param>
	/// <returns>The newly added comment, or <c>null</c> if the article was not found.</returns>
	Task<Comment?> AddCommentAsync(Guid articleId, AddCommentRequest request, Guid userId, string userEmail);

	/// <summary>
	/// Deletes a comment from an article.
	/// </summary>
	/// <param name="articleId">The unique identifier of the article containing the comment.</param>
	/// <param name="commentId">The unique identifier of the comment to delete.</param>
	/// <returns><c>true</c> if the comment was successfully deleted; otherwise, <c>false</c>.</returns>
	Task<bool> DeleteCommentAsync(Guid articleId, Guid commentId);
}