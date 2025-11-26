# Dapr Event-Based Communication

This project demonstrates event-based communication between two microservices using Dapr pub/sub.

## Architecture

- **DaprOneService.API**: Publisher service that publishes `UserCreatedEvent` events
- **DaprTwoService.API**: Subscriber service that consumes `UserCreatedEvent` events
- **DaprPlayground.Events**: Shared library containing event models
- **Dapr Pub/Sub**: Uses in-memory pub/sub component for local development

## How It Works

1. **DaprOneService.API** exposes a `POST /users` endpoint that:
   - Creates a new user
   - Publishes a `UserCreatedEvent` to the `user-created` topic via Dapr

2. **DaprTwoService.API** subscribes to the `user-created` topic and:
   - Receives the `UserCreatedEvent`
   - Logs the event details
   - Can process the event (e.g., send welcome email, create profile, etc.)

## Running the Application

1. Make sure you have Dapr CLI installed:
   ```bash
   dapr --version
   ```

2. Run the application using the AppHost:
   ```bash
   cd DaprPlayground.AppHost
   dotnet run
   ```

   This will start both services with their Dapr sidecars.

## Testing the Event Flow

### Using curl:

```bash
# Create a user (publishes event)
curl -X POST http://localhost:<daproneservice-port>/users \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "John Doe",
    "email": "john.doe@example.com"
  }'
```

### Using the .http file:

Add to `DaprOneService.API.http`:

```http
### Create User and Publish Event
POST {{DaprOneService_API_HostAddress}}/users
Content-Type: application/json

{
  "userName": "John Doe",
  "email": "john.doe@example.com"
}
```

## What Happens

1. When you POST to `/users`, DaprOneService creates a user and publishes a `UserCreatedEvent`
2. The event is sent to the Dapr sidecar via the pub/sub API
3. Dapr routes the event to all subscribers of the `user-created` topic
4. DaprTwoService receives the event at its `/user-created` endpoint
5. DaprTwoService logs the user information and processes the event

## Components

### Pub/Sub Component

Located at: `DaprPlayground.AppHost/dapr/components/pubsub.yaml`

Uses in-memory pub/sub for local development. For production, you can switch to:
- Redis
- Azure Service Bus
- RabbitMQ
- Kafka
- etc.

Simply update the component YAML file.

## Event Model

```csharp
public record UserCreatedEvent(
    Guid UserId,
    string UserName,
    string Email,
    DateTime CreatedAt
);
```

## Key Features

- ✅ Decoupled services via event-driven architecture
- ✅ Dapr pub/sub abstraction (vendor-agnostic)
- ✅ Automatic retries and error handling
- ✅ .NET Aspire integration for orchestration
- ✅ Shared event models in separate library
