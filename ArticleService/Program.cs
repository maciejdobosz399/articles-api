using ArticleService;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Wolverine;
using Wolverine.AzureServiceBus;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var appConfigConnectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");

builder.Configuration.AddAzureAppConfiguration(options =>
{
	options.Connect(appConfigConnectionString)
		   .Select("ArticleService:*", LabelFilter.Null)
		   .Select("ArticleService:*", builder.Environment.EnvironmentName)
		   .TrimKeyPrefix("ArticleService:")
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

var dbConnString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(dbConnString))
	throw new ArgumentNullException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDatabase(dbConnString)
	.AddApiVersioningConfiguration()
	.AddSwaggerDocumentation()
	.AddAuthenticationAndAuthorization(builder.Configuration)
	.AddApplicationServices()
	.AddHealthCheckServices(builder.Configuration, dbConnString);

builder.UseWolverine(opts =>
{
	var azureServiceBusConnectionString =
		builder.Configuration.GetConnectionString("AzureServiceBus");

	opts.UseAzureServiceBus(azureServiceBusConnectionString, azure =>
	{
		azure.RetryOptions.Mode = ServiceBusRetryMode.Exponential;
	}).SystemQueuesAreEnabled(false);

	opts.PublishAllMessages().ToAzureServiceBusQueue("article-queue");

	opts.PersistMessagesWithPostgresql(dbConnString, "wolverine");
	opts.Policies.UseDurableLocalQueues();
	opts.UseEntityFrameworkCoreTransactions();
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHostedService<DatabaseInitializerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseSwaggerUI(options =>
	{
		foreach (var description in app.DescribeApiVersions())
		{
			options.SwaggerEndpoint($"/openapi/{description.GroupName}.json", description.GroupName);
		}
	});
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
