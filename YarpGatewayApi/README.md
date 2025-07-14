# Building a YARP API Gateway (Proof of Concept) with .NET 8

## 🚀 Getting Started with YARP

This guide demonstrates how to build a lightweight, customizable API Gateway using **YARP (Yet Another Reverse Proxy)** on **.NET 8.0**. YARP is ideal for .NET developers who want full control over routing, middleware, and policies in code.

### Step 1: Create a New Gateway Project

```bash
dotnet new web -n Gateway
cd Gateway
```

### Step 2: Add YARP NuGet Package

```bash
dotnet add package Yarp.ReverseProxy
```

### Step 3: Enable YARP in `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();
app.Run();
```

### Step 4: Define Routes and Clusters in `appsettings.json`

> Place this in `Gateway/appsettings.json`.

```json
{
  "ReverseProxy": {
    "Routes": [
      {
        "RouteId": "example",
        "ClusterId": "exampleCluster",
        "Match": {
          "Path": "/api/example/{**catch-all}"
        }
      }
    ],
    "Clusters": {
      "exampleCluster": {
        "Destinations": {
          "exampleDestination": {
            "Address": "https://localhost:7001/"
          }
        }
      }
    }
  }
}
```

### Step 5: Run and Test

```bash
dotnet run
```

Use Postman or `curl` to test: `https://localhost:5001/api/example/your-path`

---

## 🎯 Objective

This POC showcases how YARP can be used to implement:

- 🔐 Authentication & Authorization
- 🧩 API Versioning
- 🚦 Rate Limiting
- 🔒 SSL Termination

---

## 🧱 Prerequisites & Project Structure

### Requirements

- .NET 8.0 SDK
- Visual Studio 2022 / VS Code
- HTTPS dev certificate installed (`dotnet dev-certs https --trust`)
- Postman or `curl`

### Folder Structure

```bash
/YarpGatewayPOC
├── Gateway/                    # YARP Gateway
│   └── appsettings.json
├── Services/
│   ├── ApiV1/                  # API v1
│   └── ApiV2/                  # API v2
└── README.md
```

---
## 🔐 Authentication & Authorization

### ✅ Step 1: Add JWT Auth Package

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### ✅ Step 2: Configure JWT in `Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "Your ValidIssuer",

            ValidateAudience = true,
            ValidAudience = "Your ValidAudience",

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("Your Secret key")),

            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUsers", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

app.UseAuthentication();
app.UseAuthorization();
```

### ✅ Step 3: Secure Routes in YARP Config

```json
"Routes": [
  {
    "RouteId": "v1",
    "ClusterId": "v1Cluster",
    "AuthorizationPolicy": "AuthenticatedUsers",
    "Match": {
      "Path": "/api/v1/{**catch-all}"
    }
  }
]
```

### ✅ Step 4: Generate Your JWT Token Online

Use [https://codesamplez.com/tools/jwt-builder](https://codesamplez.com/tools/jwt-builder) to generate your JWT token by filling in the claims.

#### ✍️ Fill Out the Claims

```
Identity Claims
  Issuer (iss):        echannel.com
  Subject (sub):       ddd545451
  Audience (aud):      13/3

Timing Claims
  Expiration Time (exp): 2026-12-26T08:30:30Z
  Issued At (iat):       2025-06-30T09:06:21Z
  Not Before (nbf):      2025-06-30T09:06:21Z

Metadata Claims
  JWT ID (jti):        your-identifier

Signature
  Signature Key:       H1KoIWsxmPm-2wP3xWn77fHRRExCeA8_
  Algorithm:           HS256 (HMAC with SHA-256)
```

🎯 After entering this, click **"Build JWT"** to get the token.

### ✅ Step 5: Verify and Encode Your JWT at jwt.io

Use [https://jwt.io/](https://jwt.io/) to inspect and verify the token.

#### Paste Your Encoded JWT

```text
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.
KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30
```

#### 🔓 Decoded JWT Header

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

#### 📦 Decoded JWT Payload

```json
{
  "sub": "1234567890",
  "name": "John Doe",
  "admin": true,
  "iat": 1516239022
}
```

#### 🔐 Signature Verification

Enter the **secret key** used to sign the JWT (same as step 1):

```
H1KoIWsxmPm-2wP3xWn77fHRRExCeA8_
```

✅ Once verified, you can **copy the full JWT token** and use it in your requests to the YARP API Gateway.

### ✅ Step 6: Test with cURL

```bash
curl "https://localhost:5001/api/v1/values" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3OTgyNzM4MzAsImlhdCI6MTc1MDg0MDIzMCwibmJmIjoxNzUwODQwMjMwLCJpc3MiOiJlY2hhbm5lbC5jb20iLCJzdWIiOiJkZGQ1NDU0NTEiLCJhdWQiOiIxMy8zIiwianRpIjoieW91ci1pbmRlbnRpZmllciJ9.2gnxjv4UTxExHWzwGrLJ6esEwBGosqq_Y_DKkgkGo3Q" \
  -k
```
---
## 🧹 API Versioning


## 🔁 Use Transforms for API Versioning in YARP

When implementing API versioning (like `/api/v1/products`), it’s often useful to modify the path before forwarding to the backend. This is where **Transforms** come in.

### ✅ Why Use Transforms?

Transforms let you:

- Remove the version prefix from the path before forwarding (e.g., remove `/api/v1`)
- Add or modify headers (e.g., set `X-Proxy-Version: v1`)
- Rewrite the path or preserve the original host
- Add security or diagnostic headers

---

### ✅ Step 1: Update `appsettings.json` With Transforms

Here’s an example of using **transforms** for both v1 and v2 routes:

```jsonc
"Routes": [
  {
    "RouteId": "v1",
    "ClusterId": "v1Cluster",
    "Match": {
      "Path": "/api/v1/{**catch-all}"
    },
    "AuthorizationPolicy": "AuthenticatedUsers",
    "RateLimiterPolicy": "customPolicy",
    "Transforms": [
      { "PathRemovePrefix": "/api/v1" },
      { "RequestHeader": "X-Proxy-Version", "Set": "v1" }
    ]
  },
  {
    "RouteId": "v2",
    "ClusterId": "v2Cluster",
    "Match": {
      "Path": "/api/v2/{**catch-all}"
    },
    "RateLimiterPolicy": "customPolicy",
    "Transforms": [
      { "PathRemovePrefix": "/api/v2" },
      { "RequestHeader": "X-Proxy-Version", "Set": "v2" }
    ]
  }
]
```

---

### 🔍 What Each Transform Does

| Transform                         | Description                                                              |
| --------------------------------- | ------------------------------------------------------------------------ |
| `PathRemovePrefix`                | Strips the version segment (`/api/v1`) before forwarding                 |
| `PathPrefix`                      | Adds `/apis` prefix to the modified path                                 |
| `RequestHeader: Set`              | Adds `X-Proxy-Version: v1` or `v2` for backend routing or logging        |
| `RequestHeader: Append`           | Adds value to existing header (e.g., append "bar" to `header1`)          |
| `ResponseHeader: Append`          | Modifies response headers for trace/debug (can include CORS, etc.)       |
| `ClientCert`                      | Forwards client cert in header (useful in mutual TLS setups)             |
| `RequestHeadersCopy: true`        | Copies all original headers as-is to backend                             |
| `RequestHeaderOriginalHost: true` | Keeps the original Host (e.g., for multi-tenant routing)                 |
| `X-Forwarded`                     | Adds standard headers: `X-Forwarded-For`, `-Proto`, `-Host`, `-PathBase` |

---


```jsonc
"Routes": [
  {
    "RouteId": "v1",
    "ClusterId": "v1Cluster",
    "Match": {
      "Path": "/api/v1/{**catch-all}"
    },
    "AuthorizationPolicy": "AuthenticatedUsers",
    "RateLimiterPolicy": "customPolicy",
    "Transforms": [
      { "PathRemovePrefix": "/api/v1" },
      { "PathPrefix": "/apis" },
      { "RequestHeader": "X-Proxy-Version", "Set": "v1" },
      { "RequestHeader": "header1", "Append": "bar" },
      { "ResponseHeader": "header2", "Append": "bar", "When": "Always" },
      { "ClientCert": "X-Client-Cert" },
      { "RequestHeadersCopy": "true" },
      { "RequestHeaderOriginalHost": "true" },
      {
        "X-Forwarded": "Append",
        "HeaderPrefix": "X-Forwarded-"
      }
    ]
  },
  {
    "RouteId": "v2",
    "ClusterId": "v2Cluster",
    "Match": {
      "Path": "/api/v2/{**catch-all}"
    },
    "RateLimiterPolicy": "customPolicy",
    "Transforms": [
      { "PathRemovePrefix": "/api/v2" },
      { "PathPrefix": "/apis" },
      { "RequestHeader": "X-Proxy-Version", "Set": "v2" },
      { "RequestHeader": "header1", "Append": "bar" },
      { "ResponseHeader": "header2", "Append": "bar", "When": "Always" },
      { "ClientCert": "X-Client-Cert" },
      { "RequestHeadersCopy": "true" },
      { "RequestHeaderOriginalHost": "true" },
      {
        "X-Forwarded": "Append",
        "HeaderPrefix": "X-Forwarded-"
      }
    ]
  }
]
```

---

### 🔧 Example: Incoming vs Forwarded

#### **Incoming Request**

```
GET /api/v1/products
Host: my-gateway.com
```

#### **After Transforms**

```
Forwarded to backend:
GET /apis/products
Host: my-gateway.com
Headers:
  X-Proxy-Version: v1
  header1: bar
  X-Forwarded-For: <client-ip>
```

---

## ✅ Summary: Versioning with Transforms

| Goal                        | How to Do It                         |
| --------------------------- | ------------------------------------ |
| Remove version prefix       | `PathRemovePrefix`                   |
| Add API base path           | `PathPrefix`                         |
| Indicate version to backend | `RequestHeader: X-Proxy-Version`     |
| Support diagnostics         | `X-Forwarded`, custom headers        |
| Keep headers/host intact    | `RequestHeadersCopy`, `OriginalHost` |

---


## 🚦 Rate Limiting

### 1. Configure Rate Limiting Middleware

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("customPolicy", config =>
    {
        config.PermitLimit = 5;
        config.Window = TimeSpan.FromSeconds(10);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });
});

app.UseRateLimiter();
```

### 2. Apply Policy to Reverse Proxy

```csharp
app.MapReverseProxy().RequireRateLimiting("customPolicy");
```

---

## 🔒 SSL Termination

### 1. Development Trust

```bash
dotnet dev-certs https --trust
```

### 2. Launch Settings

Edit `Gateway/Properties/launchSettings.json`:

```json
"applicationUrl": "https://localhost:5001;http://localhost:5000"
```

### 3. Production Suggestion

Use Kestrel or Nginx in front for SSL termination if deploying.

---

## ⚙️ Example YARP Config

```json
{
  "ReverseProxy": {
    "Routes": [
      {
        "RouteId": "v1",
        "ClusterId": "v1Cluster",
        "Match": {
          "Path": "/api/v1/{**catch-all}"
        },
        "AuthorizationPolicy": "Default"
      },
      {
        "RouteId": "v2",
        "ClusterId": "v2Cluster",
        "Match": {
          "Path": "/api/v2/{**catch-all}"
        }
      }
    ],
    "Clusters": {
      "v1Cluster": {
        "Destinations": {
          "v1Destination": {
            "Address": "https://localhost:7001/"
          }
        }
      },
      "v2Cluster": {
        "Destinations": {
          "v2Destination": {
            "Address": "https://localhost:7002/"
          }
        }
      }
    }
  }
}
```



---

## Dynamic Swagger Aggregation

Many microservice landscapes expose their own individual Swagger/OpenAPI documents. For a smoother developer experience, you can aggregate those documents behind the YARP gateway and surface them through a **single, dynamic Swagger UI** instance.

### Why Aggregate Swagger?

- **Zero‑config discovery** – every service that the gateway knows about automatically appears in Swagger UI.
- **Version‑aware** – supports `/swagger/v1`, `/swagger/v2`, etc., without code changes.
- **Cached & efficient** – reduces repeated back‑end calls by caching documents in‑memory.

### Implementation

Create a helper class called `SwaggerAggregator` and drop in the following code:

```csharp
using System.Text.Json;
using Yarp.ReverseProxy.Configuration;
using Microsoft.Extensions.Caching.Memory;

public static class SwaggerAggregator
{
    private static readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public static void UseDynamicSwaggerUI(WebApplication app)
    {
        // Only enable in Development
        if (!app.Environment.IsDevelopment()) return;

        // Pull the proxy configuration (routes, clusters)
        var proxyConfig = app.Services.GetRequiredService<IProxyConfigProvider>().GetConfig();
        var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();

        var swaggerEndpoints = new List<(string Name, string Url)>();

        // Loop through every declared route
        foreach (var route in proxyConfig.Routes)
        {
            var routeId   = route.RouteId;
            var clusterId = route.ClusterId;

            // Detect any PathRemovePrefix transform
            var pathRemovePrefix = route.Transforms?
                .FirstOrDefault(t => t.ContainsKey("PathRemovePrefix"))?["PathRemovePrefix"] ?? string.Empty;

            /*
             * Map a dynamic endpoint like:
             *   /swagger-json/{routeId}/swagger/{service}/swagger.json
             * This allows ANY service name (v1, v2, etc.) without hard‑coding.
             */
            var routePattern = $"/swagger-json/{routeId}/swagger/{{service}}/swagger.json";

            // Register a default endpoint for Swagger UI dropdown
            swaggerEndpoints.Add(($"{routeId} API", $"/swagger-json/{routeId}/swagger/v1/swagger.json"));

            // Actual endpoint implementation
            app.Map(routePattern, async context =>
            {
                var service  = context.Request.RouteValues["service"]?.ToString();
                var cacheKey = $"swagger-{routeId}-{service}";

                var cluster = proxyConfig.Clusters.FirstOrDefault(c => c.ClusterId == clusterId);
                if (cluster == null || cluster.Destinations.Count == 0)
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                // Pick a random destination (simple round‑robin‑ish)
                var dest           = cluster.Destinations.Values.OrderBy(_ => Guid.NewGuid()).First();
                var backendUrl     = dest.Address.TrimEnd('/');
                var backendSwagger = $"{backendUrl}/swagger/{service}/swagger.json";

                // Cache to avoid hammering back‑end services
                if (!_cache.TryGetValue(cacheKey, out string swaggerJson))
                {
                    try
                    {
                        var httpClient = httpClientFactory.CreateClient();
                        swaggerJson    = await httpClient.GetStringAsync(backendSwagger);
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
                    // Parse and tweak the OpenAPI document
                    var root          = JsonDocument.Parse(swaggerJson).RootElement;
                    var modifiedPaths = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

                    foreach (var path in root.GetProperty("paths").EnumerateObject())
                    {
                        var finalPath = !string.IsNullOrEmpty(pathRemovePrefix) &&
                                        !path.Name.StartsWith(pathRemovePrefix)
                                        ? pathRemovePrefix + path.Name
                                        : path.Name;
                        modifiedPaths[finalPath] = path.Value;
                    }

                    // Re‑build the OpenAPI JSON with updated paths
                    var openApi = new Dictionary<string, object>();
                    foreach (var prop in root.EnumerateObject())
                    {
                        openApi[prop.Name] = prop.NameEquals("paths")
                            ? (object)modifiedPaths
                            : JsonSerializer.Deserialize<object>(prop.Value.GetRawText())!;
                    }

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

        // Standard Swagger middlewares
        app.UseSwagger();

        app.UseSwaggerUI(o =>
        {
            foreach (var (name, url) in swaggerEndpoints)
                o.SwaggerEndpoint(url, name);

            o.RoutePrefix = "swagger"; // Access via http://localhost:5000/swagger
        });
    }
}
```

### Wiring It Up in `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// YARP, JWT, Rate Limiter, etc. configuration …

// 🔑 Add the minimal Swagger services (UI only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔌 Enable the dynamic Swagger aggregator
SwaggerAggregator.UseDynamicSwaggerUI(app);

app.MapReverseProxy();
app.Run();
```

### How It Works

1. \*\*Discovery \*\* – Reads the in‑memory proxy configuration (routes & clusters).
2. \*\*Dynamic endpoints \*\* – Maps `/swagger-json/{routeId}/swagger/{service}/swagger.json` for every route.
3. \*\*Caching \*\* – Keeps Swagger docs for 5 minutes to improve latency.
4. \*\*Path rewrite \*\* – Re‑adds any `PathRemovePrefix` so the exposed paths match gateway routes.
5. **Swagger UI** – Registers a dropdown entry for each route so developers can browse every service from one place.


---
## ❗ Common Issues & Troubleshooting

| Issue                  | Solution                                   |
| ---------------------- | ------------------------------------------ |
| 404 Not Found          | Check routing paths and config             |
| JWT fails              | Verify Authority URL and audience settings |
| SSL errors             | Trust dev cert or use a valid cert         |
| Rate limit not applied | Confirm middleware is enabled              |

---

## 🆚 Gateway Alternatives

| Feature           | YARP       | Kong        | NGINX     | Envoy      |
| ----------------- | ---------- | ----------- | --------- | ---------- |
| Language          | .NET (C#)  | Go/Lua      | C         | C++        |
| Custom Middleware | ✅ Full C#  | ⚠️ Limited  | ⚙️ Manual | ⚙️ C++     |
| Rate Limiting     | ✅ Built-in  | ✅ Built-in  | ✅ Module  | ✅ Built-in |
| SSL Termination   | ✅ Yes      | ✅ Yes       | ✅ Yes     | ✅ Yes      |
| UI Dashboard      | ❌ No       | ✅ Yes       | ❌ No      | ⚠️ Minimal |
| Community Size    | Medium     | Large       | Huge      | Medium     |
| Learning Curve    | Low (.NET) | Medium      | Steep     | Steep      |
| Extensibility     | ✅ High     | ⚠️ Moderate | ⚙️ Config | ✅ Code/API |

> ⚠️ = Limited via plugin/config only | ⚙️ = Requires manual configuration or coding

---

## ✅ Conclusion

YARP is an excellent option for .NET teams looking for a flexible, performant API Gateway with full C# customization. While it lacks a built-in dashboard, it excels in scenarios where control and extensibility matter most.

---

## 🛠️ Common Challenges & Solutions

| Challenge        | Solution                                                        |
| ---------------- | --------------------------------------------------------------- |
| No Dashboard     | Integrate Grafana/Prometheus or build a custom one              |
| No Auth Built-in | Use ASP.NET Core's JWT middleware as shown                      |
| Plugin Ecosystem | Extend with custom C# middleware                                |

---

## ⚖️ When to Use or Avoid YARP

### ✅ Use YARP When:

- You are already using .NET
- You want full control over routing and middleware
- You prefer code/config over GUI
- You're building internal APIs or services

### ❌ Avoid YARP If:

- You need a GUI-based API gateway
- Your team prefers low-code tools
- You need plug-ins and out-of-the-box policies

