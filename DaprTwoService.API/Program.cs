using Dapr;
using DaprPlayground.Events;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Get Products endpoint
app.MapGet("/products", () =>
{
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

// Subscribe to UserCreatedEvent
app.MapPost("/user-created", [Topic("pubsub", "user-created")] async (UserCreatedEvent userEvent, ILogger<Program> logger) =>
{
    logger.LogInformation("Received UserCreatedEvent: UserId={UserId}, UserName={UserName}, Email={Email}, CreatedAt={CreatedAt}",
        userEvent.UserId, userEvent.UserName, userEvent.Email, userEvent.CreatedAt);
    
    // Process the event here (e.g., send welcome email, create user profile, etc.)
    await Task.CompletedTask;
    
    return Results.Ok();
}).WithTopic("pubsub", "user-created");

app.Run();

record Product(int Id, string Name, string Description, decimal Price);
