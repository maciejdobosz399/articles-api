using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		logger.LogError(exception, "An unhandled exception occurred.");

		var problemDetails = new ProblemDetails
		{
			Status = StatusCodes.Status500InternalServerError,
			Title = "An unexpected error occurred.",
			Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
		};

		httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

		return true;
	}
}