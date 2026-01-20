using System.Diagnostics;
using Dapr.Client;
using DaprPlayground.Events;
using Observability;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservabilityExt(builder.Configuration);
// Add Dapr client
builder.Services.AddDaprClient();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var app = builder.Build();


// Configure the HTTP request pipeline.

app.MapOpenApi();
app.MapScalarApiReference();
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

// Cache'e veri kaydetme endpoint'i
app.MapPost("/cache/{key}", async (string key, CacheItem item, DaprClient daprClient, ILogger<Program> logger) =>
{
    logger.LogInformation("Saving key {Key} to cache", key);
    
    await daprClient.SaveStateAsync("statestore", key, item);
    
    logger.LogInformation("Key {Key} saved successfully", key);
    return Results.Ok(new { Message = "Cache item saved successfully", Key = key });
}).WithName("SetCache");

// Cache'den veri okuma endpoint'i
app.MapGet("/cache/{key}", async (string key, DaprClient daprClient, ILogger<Program> logger) =>
{
    logger.LogInformation("Retrieving key {Key} from cache", key);
    
    var cachedItem = await daprClient.GetStateAsync<CacheItem>("statestore", key);
    
    if (cachedItem == null)
    {
        logger.LogWarning("Key {Key} not found in cache", key);
        return Results.NotFound(new { Message = "Key not found in cache", Key = key });
    }
    
    logger.LogInformation("Key {Key} retrieved successfully", key);
    return Results.Ok(cachedItem);
}).WithName("GetCache");

ActivitySourceProvider.Source = new ActivitySource(builder.Environment.ApplicationName);

app.Run();

internal record Product(int Id, string Name, string Description, decimal Price);

internal record CreateUserRequest(string UserName, string Email);

internal record CacheItem(string Value, DateTime CreatedAt);