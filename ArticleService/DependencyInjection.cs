using ArticleService.DbContexts;
using ArticleService.Repositories;
using ArticleService.Repositories.Interfaces;
using ArticleService.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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
		services.AddOpenApi("v1", options =>
		{
			options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
		});

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
		services.AddScoped<IArticleRepository, ArticleRepository>();
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
				"article-queue")
			.AddNpgSql(dbConnectionString);

		return services;
	}
}

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
	public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
	{
		var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

		if (authenticationSchemes.Any(authScheme => authScheme.Name == JwtBearerDefaults.AuthenticationScheme))
		{
			var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
			{
				["Bearer"] = new OpenApiSecurityScheme
				{
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					In = ParameterLocation.Header,
					BearerFormat = "Json Web Token"
				}
			};

			document.Components ??= new OpenApiComponents();
			document.Components.SecuritySchemes = securitySchemes;

			foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
			{
				operation.Value.Security ??= [];
				operation.Value.Security.Add(new OpenApiSecurityRequirement
				{
					[new OpenApiSecuritySchemeReference("Bearer", document)] = []
				});
			}
		}
	}
}
