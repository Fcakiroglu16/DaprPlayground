using System.Diagnostics;
using Dapr;
using DaprPlayground.Events;
using Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservabilityExt(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseCloudEvents();
app.MapSubscribeHandler();

// Get Products endpoint
app.MapGet("/products", async (ILogger<Product> logger) =>
{
    using (var activity = ActivitySourceProvider.Source.StartActivity("Method3"))
    {
        await Task.Delay(1000);
    }

    using (var activity = ActivitySourceProvider.Source.StartActivity("Method4"))
    {
        await Task.Delay(1000);
    }

    logger.LogInformation("Returning static list of products");

    var products = new[]
    {
        new Product(1, "Laptop", "High-performance laptop", 1299.99m),
        new Product(2, "Mouse", "Wireless ergonomic mouse", 29.99m),
        new Product(3, "Keyboard", "Mechanical keyboard", 89.99m),
        new Product(4, "Monitor", "27-inch 4K monitor", 449.99m),
        new Product(5, "Headphones", "Noise-cancelling headphones", 199.99m)
    };

    return Results.Ok(products);
}).WithName("GetProducts");

// Subscribe to UserCreatedEvent - WithTopic kaldırıldı
app.MapPost("/user-created",
    [Topic("pubsub", "user-created")] async (UserCreatedEvent userEvent, ILogger<Program> logger) =>
    {
        logger.LogInformation(
            "Received UserCreatedEvent: UserId={UserId}, UserName={UserName}, Email={Email}, CreatedAt={CreatedAt}",
            userEvent.UserId, userEvent.UserName, userEvent.Email, userEvent.CreatedAt);

        await Task.CompletedTask;
        return Results.Ok();
    });
ActivitySourceProvider.Source = new ActivitySource(builder.Environment.ApplicationName);
app.Run();

internal record Product(int Id, string Name, string Description, decimal Price);