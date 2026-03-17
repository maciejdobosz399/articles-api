using Azure.Identity;
using Azure.Messaging.ServiceBus;
using MailKit.Security;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using NotificationService.Services;
using NotificationService.Settings;
using Wolverine;
using Wolverine.AzureServiceBus;
using Wolverine.ErrorHandling;

var builder = Host.CreateApplicationBuilder(args);

var appConfigConnectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(appConfigConnectionString)
           .Select("NotificationService:*", LabelFilter.Null)
           .Select("NotificationService:*", builder.Environment.EnvironmentName)
           .TrimKeyPrefix("NotificationService:")
           .Select("Shared:*", LabelFilter.Null)
           .Select("Shared:*", builder.Environment.EnvironmentName)
           .TrimKeyPrefix("Shared:")

           .ConfigureKeyVault(kv =>
           {
               kv.SetCredential(new DefaultAzureCredential());
           })
           .ConfigureRefresh(refresh =>
           {
               refresh.Register("SentinelKey", refreshAll: true)
                      .SetRefreshInterval(TimeSpan.FromSeconds(30));
           });
});

builder.UseWolverine(opts =>
{
    var azureServiceBusConnectionString =
        builder.Configuration.GetConnectionString("AzureServiceBus");

    opts.UseAzureServiceBus(azureServiceBusConnectionString, azure =>
    {
        azure.RetryOptions.Mode = ServiceBusRetryMode.Exponential;
    }).SystemQueuesAreEnabled(false);

    opts.ListenToAzureServiceBusQueue("article-queue");
        //.CircuitBreaker();

	//commented out because my azure subscription is too small to allow for a dead letter queue, but in production you would want to move messages that fail due to an AuthenticationException to an error queue for later analysis
	//opts.Policies.OnException<AuthenticationException>().MoveToErrorQueue();
	//opts.Policies.OnException<Exception>()
	//    .RetryWithCooldown(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15));
});

builder.Services.AddOptions<EmailSettings>()
    .BindConfiguration("Smtp")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAzureAppConfiguration();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<AzureAppConfigurationRefreshService>();

var host = builder.Build();
host.Run();
