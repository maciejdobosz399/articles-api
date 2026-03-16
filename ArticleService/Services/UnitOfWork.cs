using ArticleService.DbContexts;
using ArticleService.Services.Interfaces;
using Wolverine.EntityFrameworkCore;

namespace ArticleService.Services;

public class UnitOfWork(IDbContextOutbox<ArticleDbContext> outbox) : IUnitOfWork
{
	public async Task CommitAsync(CancellationToken cancellationToken = default)
	{
		await outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);

		if (outbox.DbContext.Database.CurrentTransaction is not null)
		{
			await outbox.DbContext.Database.CurrentTransaction.CommitAsync(cancellationToken);
		}
	}
}
