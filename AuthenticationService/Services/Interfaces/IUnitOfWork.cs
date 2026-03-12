namespace AuthenticationService.Services.Interfaces;

public interface IUnitOfWork
{
	Task CommitAsync(CancellationToken cancellationToken = default);
}
