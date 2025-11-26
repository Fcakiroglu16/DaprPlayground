using Dapr.Client;
using DaprPlayground.Events;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Dapr client
builder.Services.AddDaprClient();

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

var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(index)), Random.Shared.Next(-20, 55), summaries[Random.Shared.Next(summaries.Length)])).ToArray();
    return forecast;
}).WithName("GetWeatherForecast");

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record CreateUserRequest(string UserName, string Email);
