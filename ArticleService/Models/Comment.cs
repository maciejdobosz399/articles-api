using System.Text.Json.Serialization;

namespace ArticleService.Models;

public class Comment
{
	public Guid Id { get; set; }
	public string Content { get; set; } = string.Empty;
	public Guid AuthorId { get; set; }
	public Guid ArticleId { get; set; }

	[JsonIgnore]
	public Article Article { get; set; } = null!;

	public DateTime CreatedAt { get; set; }
}