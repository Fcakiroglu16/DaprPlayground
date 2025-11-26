using System.Diagnostics;
using Dapr.Client;
using DaprOneService.API;
using DaprPlayground.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Dapr client
builder.Services.AddDaprClient();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Get products from DaprTwoService via Dapr service invocation
app.MapGet("/products", async (DaprClient daprClient) =>
{

    using (var activity= ActivitySourceProvider.ActivitySource.StartActivity("Method1"))
    {
        await Task.Delay(1000);
    }
    
    using (var activity= ActivitySourceProvider.ActivitySource.StartActivity("Method2"))
    {
        await Task.Delay(1000);
    }
    
    Product[] products = await daprClient.InvokeMethodAsync<Product[]>(
        HttpMethod.Get,
        "daprtwo-service-api",
        "products");

    return Results.Ok(products);
}).WithName("GetProductsFromServiceTwo");

// Endpoint to create a user and publish event
app.MapPost("/users", async (CreateUserRequest request, DaprClient daprClient) =>
{
    Guid userId = Guid.NewGuid();
    UserCreatedEvent userCreatedEvent = new UserCreatedEvent(
        userId,
        request.UserName,
        request.Email,
        DateTime.UtcNow
    );

    // Publish event to Dapr pub/sub
    await daprClient.PublishEventAsync("pubsub", "user-created", userCreatedEvent);

    return Results.Ok(new { UserId = userId, Message = "User created and event published" });
}).WithName("CreateUser");

ActivitySourceProvider.ActivitySource = new ActivitySource(builder.Environment.ApplicationName);

app.Run();

internal record Product(int Id, string Name, string Description, decimal Price);

internal record CreateUserRequest(string UserName, string Email);
