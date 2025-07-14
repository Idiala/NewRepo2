using System.Text.Json;
using Yarp.ReverseProxy.Configuration;
using Microsoft.Extensions.Caching.Memory;

public static class SwaggerAggregator
{
    private static readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public static void UseDynamicSwaggerUI(WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;

        var proxyConfig = app.Services.GetRequiredService<IProxyConfigProvider>().GetConfig();

        var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();

        var swaggerEndpoints = new List<(string Name, string Url)>();

        foreach (var route in proxyConfig.Routes)
        {
            var routeId = route.RouteId;
            var clusterId = route.ClusterId;

            var pathRemovePrefix = route.Transforms?
                .FirstOrDefault(t => t.ContainsKey("PathRemovePrefix"))?["PathRemovePrefix"] ?? string.Empty;

            /* -----------------------------------------------------------------
             * Instead of a fixed endpoint for v1 only, register a pattern that
             *   contains {service}. This supports any service (v1, v2, …)
             *   without changing the code.
             * ----------------------------------------------------------------*/
            var routePattern = $"/swagger-json/{routeId}/swagger/{{service}}/swagger.json";

            swaggerEndpoints.Add(($"{routeId} API",
                                  $"/swagger-json/{routeId}/swagger/v1/swagger.json"));

            //  Map the dynamic endpoint
            app.Map(routePattern, async context =>
            {
                var service = context.Request.RouteValues["service"]?.ToString();
                var cacheKey = $"swagger-{routeId}-{service}";

                var cluster = proxyConfig.Clusters.FirstOrDefault(c => c.ClusterId == clusterId);
                if (cluster == null || cluster.Destinations.Count == 0)
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                // Pick a random backend destination
                var dest = cluster.Destinations.Values.OrderBy(_ => Guid.NewGuid()).First();
                var backendUrl = dest.Address.TrimEnd('/');
                var backendSwagger = $"{backendUrl}/swagger/{service}/swagger.json";

                //  Use cache to reduce backend calls
                if (!_cache.TryGetValue(cacheKey, out string swaggerJson))
                {
                    try
                    {
                        var httpClient = httpClientFactory.CreateClient();
                        swaggerJson = await httpClient.GetStringAsync(backendSwagger);
                        _cache.Set(cacheKey, swaggerJson, TimeSpan.FromMinutes(5));
                    }
                    catch
                    {
                        context.Response.StatusCode = 502;
                        await context.Response.WriteAsync("Failed to fetch Swagger from backend.");
                        return;
                    }
                }

                try
                {
                    // Parse and adjust paths (re‑add prefix if needed)
                    var root = JsonDocument.Parse(swaggerJson).RootElement;
                    var modifiedPaths = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

                    foreach (var path in root.GetProperty("paths").EnumerateObject())
                    {
                        var finalPath = !string.IsNullOrEmpty(pathRemovePrefix) &&
                                        !path.Name.StartsWith(pathRemovePrefix)
                                        ? pathRemovePrefix + path.Name
                                        : path.Name;

                        modifiedPaths[finalPath] = path.Value;
                    }

                    //Rebuild the OpenAPI document
                    var openApi = new Dictionary<string, object>();
                    foreach (var prop in root.EnumerateObject())
                    {
                        openApi[prop.Name] = prop.NameEquals("paths")
                            ? (object)modifiedPaths
                            : JsonSerializer.Deserialize<object>(prop.Value.GetRawText())!;
                    }

                    // Send the modified document back to the client
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(openApi, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
                }
                catch (JsonException)
                {
                    context.Response.StatusCode = 500;
                }
            });
        }

        app.UseSwagger();

        app.UseSwaggerUI(o =>
        {
            foreach (var (name, url) in swaggerEndpoints)
                o.SwaggerEndpoint(url, name);

            o.RoutePrefix = "swagger";   // e.g., http://localhost:5000/swagger
        });
    }
}
