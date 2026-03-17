using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace NotificationService.Services;

public class AzureAppConfigurationRefreshService(
	IConfigurationRefresherProvider refresherProvider,
	ILogger<AzureAppConfigurationRefreshService> logger) : BackgroundService
{
	private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(30);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			foreach (var refresher in refresherProvider.Refreshers)
			{
				try
				{
					await refresher.TryRefreshAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, "Azure App Configuration refresh failed");
				}
			}

			await Task.Delay(RefreshInterval, stoppingToken);
		}
	}
}
