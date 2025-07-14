var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MyAppAspire_ApiService>("apiservice");

// Add ApiV1
var apiV1 = builder.AddProject<Projects.ApiV1>("apiv1");

// Add ApiV2
var apiV2 = builder.AddProject<Projects.ApiV2>("apiv2");


var yarpGateway = builder.AddProject<Projects.YarpGatewayApi>("yarp-gateway")
    .WithEnvironment("ASPNETCORE_FORWARDEDHEADERS_ENABLED", "true")
    .WithReference(apiV1)
    .WithReference(apiV2)
    .WithReference(apiService);

// Add Web Frontend
builder.AddProject<Projects.MyAppAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(yarpGateway)
    .WaitFor(apiService);

builder.Build().Run();