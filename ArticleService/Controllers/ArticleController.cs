using Asp.Versioning;
using ArticleService.Models;
using ArticleService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ArticleController(IArticleService articleService) : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetArticles()
	{
		var articles = await articleService.GetArticlesAsync();
		return Ok(articles);
	}

	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetArticle(Guid id)
	{
		var article = await articleService.GetArticleByIdAsync(id);
		return article is not null ? Ok(article) : NotFound();
	}

	[HttpPost]
	public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest request)
	{
		var article = await articleService.CreateArticleAsync(request);
		return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] UpdateArticleRequest request)
	{
		var article = await articleService.UpdateArticleAsync(id, request);
		return article is not null ? Ok(article) : NotFound();
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> DeleteArticle(Guid id)
	{
		var deleted = await articleService.DeleteArticleAsync(id);
		return deleted ? NoContent() : NotFound();
	}

	[HttpGet("{articleId:guid}/comments")]
	public async Task<IActionResult> GetComments(Guid articleId)
	{
		var comments = await articleService.GetCommentsAsync(articleId);
		return Ok(comments);
	}

	[HttpPost("{articleId:guid}/comments")]
	public async Task<IActionResult> AddComment(Guid articleId, [FromBody] AddCommentRequest request)
	{
		var comment = await articleService.AddCommentAsync(articleId, request);
		return comment is not null ? Created($"api/v1/article/{articleId}/comments/{comment.Id}", comment) : NotFound();
	}

	[HttpDelete("{articleId:guid}/comments/{commentId:guid}")]
	public async Task<IActionResult> DeleteComment(Guid articleId, Guid commentId)
	{
		var deleted = await articleService.DeleteCommentAsync(articleId, commentId);
		return deleted ? NoContent() : NotFound();
	}
}
