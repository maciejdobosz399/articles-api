using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/auth/v1.json", "Authentication Service v1");
        options.SwaggerEndpoint("/openapi/articles/v1.json", "Article Service v1");
    });

    app.MapGet("/openapi/auth/{**remainder}", async (string remainder, HttpContext httpContext, IHttpClientFactory httpClientFactory) =>
    {
        return await ProxyOpenApiDocument(httpContext, httpClientFactory, $"http://authenticationservice:8080/openapi/{remainder}");
    });

    app.MapGet("/openapi/articles/{**remainder}", async (string remainder, HttpContext httpContext, IHttpClientFactory httpClientFactory) =>
    {
        return await ProxyOpenApiDocument(httpContext, httpClientFactory, $"http://articleservice:8080/openapi/{remainder}");
    });
}

app.MapReverseProxy();

app.Run();

static async Task<IResult> ProxyOpenApiDocument(HttpContext httpContext, IHttpClientFactory httpClientFactory, string downstreamUrl)
{
    var client = httpClientFactory.CreateClient();
    var response = await client.GetAsync(downstreamUrl);

    if (!response.IsSuccessStatusCode)
        return Results.StatusCode((int)response.StatusCode);

    var json = await response.Content.ReadAsStringAsync();
    var doc = JsonNode.Parse(json);

    if (doc != null)
    {
        var gatewayUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        doc["servers"] = new JsonArray(new JsonObject { ["url"] = gatewayUrl });
        json = doc.ToJsonString();
    }

    return Results.Content(json, "application/json");
}
