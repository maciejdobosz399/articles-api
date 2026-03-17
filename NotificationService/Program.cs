using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using NotificationService.Services;
using NotificationService.Settings;
using Wolverine;
using Wolverine.AzureServiceBus;

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
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IEmailService, EmailService>();

var host = builder.Build();
host.Run();
