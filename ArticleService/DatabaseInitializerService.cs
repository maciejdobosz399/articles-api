using ArticleService.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ArticleService;

public class DatabaseInitializerService(
	IServiceScopeFactory scopeFactory,
	IHostEnvironment environment,
	ILogger<DatabaseInitializerService> logger) : IHostedLifecycleService
{
	public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		if (environment.IsProduction())
		{
			await ApplyMigrationsAsync(cancellationToken);
		}
	}

	public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Applying database migrations...");
		await using var scope = scopeFactory.CreateAsyncScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ArticleDbContext>();
		await dbContext.Database.MigrateAsync(cancellationToken);
		logger.LogInformation("Database migrations applied successfully.");
	}
}
