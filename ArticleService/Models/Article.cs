namespace ArticleService.Models;

public class Article
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public Guid AuthorId { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public ICollection<Comment> Comments { get; set; } = [];
}
