var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DaprTwoService_API>("daprtwoservice-api");

builder.AddProject<Projects.DaprOneService_API>("daproneservice-api");

builder.Build().Run();
