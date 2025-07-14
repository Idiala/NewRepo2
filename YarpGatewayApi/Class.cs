using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly ReverseProxyDocumentFilterConfig _reverseProxyDocumentFilterConfig;

    public ConfigureSwaggerOptions(IOptions<ReverseProxyDocumentFilterConfig> config)
    {
        _reverseProxyDocumentFilterConfig = config.Value;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var cluster in _reverseProxyDocumentFilterConfig.Clusters)
        {
            options.SwaggerDoc(cluster.Key, new OpenApiInfo { Title = cluster.Key, Version = "v1" });
        }

        options.DocumentFilter<ReverseProxyDocumentFilter>();
    }


}

public class ReverseProxyDocumentFilterConfig
{
    public Dictionary<string, Cluster> Clusters { get; set; } = new();

    public class Cluster
    {
        public Dictionary<string, Destination> Destinations { get; set; } = new();

        public class Destination
        {
            public string Address { get; set; } = string.Empty;
            public Swagger[] Swaggers { get; set; } = Array.Empty<Swagger>();

            public class Swagger
            {
                public string PrefixPath { get; set; } = string.Empty;
                public string MetadataPath { get; set; } = string.Empty;
                public string[] Paths { get; set; } = Array.Empty<string>();
            }
        }
    }
}
public class ReverseProxyDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
    }
}