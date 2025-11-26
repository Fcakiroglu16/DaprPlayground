using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var componentsPath = Path.Combine(builder.AppHostDirectory, "dapr", "components");

// Add services with Dapr sidecars
builder.AddProject<Projects.DaprOneService_API>("daproneservice-api")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "daproneservice-api",
        ResourcesPaths = [componentsPath]
    });

builder.AddProject<Projects.DaprTwoService_API>("daprtwoservice-api")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "daprtwoservice-api",
        ResourcesPaths = [componentsPath]
    });

builder.Build().Run();
