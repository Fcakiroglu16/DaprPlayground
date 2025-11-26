using CommunityToolkit.Aspire.Hosting.Dapr;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var componentsPath = Path.Combine(builder.AppHostDirectory, "dapr", "components");

// Add services with Dapr sidecars
builder.AddProject<Projects.DaprOneService_API>("daproneservice-api")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "daprone-service-api",
        ResourcesPaths = [componentsPath]
    });

builder.AddProject<Projects.DaprTwoService_API>("daprtwoservice-api")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "daprtwo-service-api",
        ResourcesPaths = [componentsPath]
    });

builder.Build().Run();
