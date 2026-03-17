using ArticleService.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.DbContexts;

public class ArticleDbContext(DbContextOptions<ArticleDbContext> options) : DbContext(options)
{
	public DbSet<Article> Articles { get; set; }
	public DbSet<Comment> Comments { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Article>(entity =>
		{
			entity.HasKey(a => a.Id);
			entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
			entity.Property(a => a.Content).IsRequired();
			entity.HasMany(a => a.Comments)
				  .WithOne(c => c.Article)
				  .HasForeignKey(c => c.ArticleId)
				  .OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<Comment>(entity =>
		{
			entity.HasKey(c => c.Id);
			entity.Property(c => c.Content).IsRequired().HasMaxLength(1000);
		});
	}
}