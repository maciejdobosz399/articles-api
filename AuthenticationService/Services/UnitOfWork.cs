using AuthenticationService.DbContexts;
using AuthenticationService.Services.Interfaces;
using Wolverine.EntityFrameworkCore;

namespace AuthenticationService.Services;

public class UnitOfWork(IDbContextOutbox<ApplicationDbContext> outbox) : IUnitOfWork
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
