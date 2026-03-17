using ArticleService.DbContexts;
using ArticleService.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ArticleService;

public static class DependencyInjection
{
	public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
	{
		services.AddDbContext<ArticleDbContext>(options =>
		{
			options.UseNpgsql(connectionString);
		});

		return services;
	}

	public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
	{
		services.AddApiVersioning(options =>
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

		return services;
	}

	public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
	{
		services.AddOpenApi("v1");

		return services;
	}

	public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddAuthentication(options =>
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
				ValidIssuer = configuration["JwtSettings:Issuer"],
				ValidAudience = configuration["JwtSettings:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!)),
				ClockSkew = TimeSpan.Zero,
				NameClaimType = JwtRegisteredClaimNames.Sub,
				RoleClaimType = "role"
			};
		});

		services.AddAuthorization();

		return services;
	}

	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		services.AddScoped<IArticleService, Services.ArticleService>();
		services.AddScoped<IUnitOfWork, Services.UnitOfWork>();

		return services;
	}

	public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration, string dbConnectionString)
	{
		services.AddHealthChecks()
			.AddDbContextCheck<ArticleDbContext>()
			.AddAzureServiceBusQueue(
				configuration.GetConnectionString("AzureServiceBus")!,
				"notification-queue")
			.AddNpgSql(dbConnectionString);

		return services;
	}
}
