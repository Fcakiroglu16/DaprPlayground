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
