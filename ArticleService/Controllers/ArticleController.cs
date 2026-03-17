using ArticleService.Models;
using ArticleService.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ArticleService.Controllers;

/// <summary>
/// Manages articles and their comments.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/articles")]
public class ArticleController(IArticleService articleService) : ControllerBase
{
	private Guid GetUserId() =>
		Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

	private string GetUserEmail() =>
		User.FindFirst(JwtRegisteredClaimNames.Email)!.Value;

	/// <summary>
	/// Retrieves all articles.
	/// </summary>
	/// <returns>A list of all articles.</returns>
	/// <response code="200">Returns the list of articles.</response>
	[HttpGet]
	[AllowAnonymous]
	[ProducesResponseType(typeof(IEnumerable<Article>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetArticles()
	{
		var articles = await articleService.GetArticlesAsync();
		return Ok(articles);
	}

	/// <summary>
	/// Retrieves a specific article by its unique identifier.
	/// </summary>
	/// <param name="id">The unique identifier of the article.</param>
	/// <returns>The requested article.</returns>
	/// <response code="200">Returns the requested article.</response>
	/// <response code="404">Article not found.</response>
	[HttpGet("{id:guid}")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(Article), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetArticle(Guid id)
	{
		var article = await articleService.GetArticleByIdAsync(id);
		return article is not null ? Ok(article) : NotFound();
	}

	/// <summary>
	/// Creates a new article. Requires authentication.
	/// </summary>
	/// <param name="request">The article creation details including title and content.</param>
	/// <returns>The newly created article.</returns>
	/// <response code="201">Article created successfully.</response>
	/// <response code="401">User is not authenticated.</response>
	[HttpPost]
	[ProducesResponseType(typeof(Article), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest request)
	{
		var article = await articleService.CreateArticleAsync(request, GetUserId(), GetUserEmail());
		return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
	}

	/// <summary>
	/// Updates an existing article. Requires authentication and ownership.
	/// </summary>
	/// <param name="id">The unique identifier of the article to update.</param>
	/// <param name="request">The updated article details including title and content.</param>
	/// <returns>The updated article.</returns>
	/// <response code="200">Article updated successfully.</response>
	/// <response code="401">User is not authenticated.</response>
	/// <response code="404">Article not found.</response>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(typeof(Article), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] UpdateArticleRequest request)
	{
		var article = await articleService.UpdateArticleAsync(id, request, GetUserId());
		return article is not null ? Ok(article) : NotFound();
	}

	/// <summary>
	/// Deletes an article. Requires authentication and ownership.
	/// </summary>
	/// <param name="id">The unique identifier of the article to delete.</param>
	/// <returns>No content on success.</returns>
	/// <response code="204">Article deleted successfully.</response>
	/// <response code="401">User is not authenticated.</response>
	/// <response code="404">Article not found.</response>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> DeleteArticle(Guid id)
	{
		var deleted = await articleService.DeleteArticleAsync(id, GetUserId());
		return deleted ? NoContent() : NotFound();
	}

	/// <summary>
	/// Retrieves all comments for a specific article.
	/// </summary>
	/// <param name="articleId">The unique identifier of the article.</param>
	/// <returns>A list of comments for the specified article.</returns>
	/// <response code="200">Returns the list of comments.</response>
	[HttpGet("{articleId:guid}/comments")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(IEnumerable<Comment>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetComments(Guid articleId)
	{
		var comments = await articleService.GetCommentsAsync(articleId);
		return Ok(comments);
	}

	/// <summary>
	/// Adds a comment to an article. Requires authentication.
	/// </summary>
	/// <param name="articleId">The unique identifier of the article to comment on.</param>
	/// <param name="request">The comment details including content.</param>
	/// <returns>The newly created comment.</returns>
	/// <response code="201">Comment added successfully.</response>
	/// <response code="401">User is not authenticated.</response>
	/// <response code="404">Article not found.</response>
	[HttpPost("{articleId:guid}/comments")]
	[ProducesResponseType(typeof(Comment), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> AddComment(Guid articleId, [FromBody] AddCommentRequest request)
	{
		var comment = await articleService.AddCommentAsync(articleId, request, GetUserId(), GetUserEmail());
		return comment is not null ? Created($"api/v1/article/{articleId}/comments/{comment.Id}", comment) : NotFound();
	}

	/// <summary>
	/// Deletes a comment from an article. Requires authentication.
	/// </summary>
	/// <param name="articleId">The unique identifier of the article.</param>
	/// <param name="commentId">The unique identifier of the comment to delete.</param>
	/// <returns>No content on success.</returns>
	/// <response code="204">Comment deleted successfully.</response>
	/// <response code="401">User is not authenticated.</response>
	/// <response code="404">Article or comment not found.</response>
	[HttpDelete("{articleId:guid}/comments/{commentId:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> DeleteComment(Guid articleId, Guid commentId)
	{
		var deleted = await articleService.DeleteCommentAsync(articleId, commentId);
		return deleted ? NoContent() : NotFound();
	}
}