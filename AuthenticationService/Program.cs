using AuthenticationService;
using AuthenticationService.DbContexts;
using AuthenticationService.Services;
using AuthenticationService.Services.Interfaces;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Wolverine;
using Wolverine.AzureServiceBus;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");

builder.Configuration.AddAzureAppConfiguration(options =>
{
	options.Connect(connectionString)
		   .Select(KeyFilter.Any, LabelFilter.Null)
		   .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseNpgsql(dbConnString);
});

builder.Services.AddApiVersioningConfiguration()
	.AddSwaggerDocumentation()
	.AddAuthenticationAndAuthorization(builder.Configuration);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.UseWolverine(opts =>
{
	var azureServiceBusConnectionString =
		builder.Configuration.GetConnectionString("AzureServiceBus");

	var dbConnectionString =
		builder.Configuration.GetConnectionString("DefaultConnection");

	opts.UseAzureServiceBus(azureServiceBusConnectionString, azure =>
	{
		azure.RetryOptions.Mode = ServiceBusRetryMode.Exponential;
	}).SystemQueuesAreEnabled(false);

	opts.PublishAllMessages().ToAzureServiceBusQueue("article-queue");

	opts.PersistMessagesWithPostgresql(dbConnectionString, "wolverine");
	opts.Policies.UseDurableLocalQueues();
	opts.UseEntityFrameworkCoreTransactions();
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddAzureServiceBusQueue(
        builder.Configuration.GetConnectionString("AzureServiceBus")!,
        "article-queue")
    .AddNpgSql(dbConnString);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHostedService<DatabaseInitializerService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

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
app.UseHsts();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
