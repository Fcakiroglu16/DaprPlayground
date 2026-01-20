using System.Diagnostics;
using Dapr.Client;
using DaprPlayground.Events;
using Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservabilityExt(builder.Configuration);
// Add Dapr client
builder.Services.AddDaprClient();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

// Get products from DaprTwoService via Dapr service invocation
app.MapGet("/products", async (DaprClient daprClient, ILogger<Product> logger) =>
{
    using (var activity = ActivitySourceProvider.Source.StartActivity("Method1"))
    {
        await Task.Delay(1000);
    }

    using (var activity = ActivitySourceProvider.Source.StartActivity("Method2"))
    {
        await Task.Delay(1000);
    }

    //logging
    logger.LogInformation("Invoking DaprTwoService to get products");
    var products = await daprClient.InvokeMethodAsync<Product[]>(
        HttpMethod.Get,
        "daprtwo-service-api",
        "products");


    logger.LogInformation("Resulting DaprTwoService to get products");
    return Results.Ok(products);
}).WithName("GetProductsFromServiceTwo");

// Endpoint to create a user and publish event
app.MapPost("/users", async (CreateUserRequest request, DaprClient daprClient) =>
{
    var userId = Guid.NewGuid();
    var userCreatedEvent = new UserCreatedEvent(
        userId,
        request.UserName,
        request.Email,
        DateTime.UtcNow
    );

    // Publish event to Dapr pub/sub
    await daprClient.PublishEventAsync("pubsub", "user-created", userCreatedEvent);

    return Results.Ok(new { UserId = userId, Message = "User created and event published" });
}).WithName("CreateUser");

ActivitySourceProvider.Source = new ActivitySource(builder.Environment.ApplicationName);

app.Run();

internal record Product(int Id, string Name, string Description, decimal Price);

internal record CreateUserRequest(string UserName, string Email);