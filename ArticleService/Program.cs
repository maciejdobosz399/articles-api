using ArticleService;
using ArticleService.DbContexts;
using ArticleService.Services.Interfaces;
using Asp.Versioning;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
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

builder.Services.AddDbContext<ArticleDbContext>(options =>
{
	options.UseNpgsql(dbConnString);
});

builder.Services.AddApiVersioning(options =>
{
	options.DefaultApiVersion = new ApiVersion(1, 0);
	options.AssumeDefaultVersionWhenUnspecified = true;
	options.ReportApiVersions = true;
	options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
	options.GroupNameFormat = "'v'VVV";
	options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddOpenApi("v1");

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.MapInboundClaims = false;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
		ValidAudience = builder.Configuration["JwtSettings:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
		ClockSkew = TimeSpan.Zero,
		NameClaimType = JwtRegisteredClaimNames.Sub,
		RoleClaimType = "role"
	};
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IArticleService, ArticleService.Services.ArticleService>();
builder.Services.AddScoped<IUnitOfWork, ArticleService.Services.UnitOfWork>();

builder.UseWolverine(opts =>
{
	var azureServiceBusConnectionString =
		builder.Configuration.GetConnectionString("AzureServiceBus");

	opts.UseAzureServiceBus(azureServiceBusConnectionString, azure =>
	{
		azure.RetryOptions.Mode = ServiceBusRetryMode.Exponential;
	}).SystemQueuesAreEnabled(false);

	opts.PublishAllMessages().ToAzureServiceBusQueue("notification-queue");

	opts.PersistMessagesWithPostgresql(dbConnString, "wolverine");
	opts.Policies.UseDurableLocalQueues();
	opts.UseEntityFrameworkCoreTransactions();
});

builder.Services.AddHealthChecks()
	.AddDbContextCheck<ArticleDbContext>()
	.AddAzureServiceBusQueue(
		builder.Configuration.GetConnectionString("AzureServiceBus")!,
		"notification-queue")
	.AddNpgSql(dbConnString);

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
