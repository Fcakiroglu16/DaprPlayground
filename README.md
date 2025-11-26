# Dapr Communication Patterns

This project demonstrates two key Dapr communication patterns between microservices:
1. **Pub/Sub** (Asynchronous event-based communication)
2. **Service Invocation** (Synchronous service-to-service calls)

## Architecture

- **DaprOneService.API**: 
  - Publishes `UserCreatedEvent` events (Pub/Sub)
  - Calls DaprTwoService to get products (Service Invocation)
- **DaprTwoService.API**: 
  - Subscribes to `UserCreatedEvent` events (Pub/Sub)
  - Provides products endpoint (Service Invocation)
- **DaprPlayground.Events**: Shared library containing event models
- **Dapr Components**: In-memory pub/sub for local development

## Communication Patterns

### 1. Pub/Sub (Asynchronous)

**DaprOneService.API** exposes a `POST /users` endpoint that:
- Creates a new user
- Publishes a `UserCreatedEvent` to the `user-created` topic via Dapr

**DaprTwoService.API** subscribes to the `user-created` topic and:
- Receives the `UserCreatedEvent`
- Logs the event details
- Processes the event (e.g., send welcome email, create profile, etc.)

### 2. Service Invocation (Synchronous)

**DaprTwoService.API** exposes a `GET /products` endpoint that:
- Returns a list of sample products

**DaprOneService.API** exposes a `GET /products` endpoint that:
- Calls DaprTwoService via Dapr service invocation
- Returns the products from DaprTwoService

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

## Testing the Communication Patterns

### Service Invocation (Get Products):

```bash
# Get products via DaprOneService (calls DaprTwoService internally)
curl http://localhost:5015/products

# Or call DaprTwoService directly
curl http://localhost:5076/products
```

### Pub/Sub (Create User Event):

```bash
# Create a user (publishes event)
curl -X POST http://localhost:5015/users \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "John Doe",
    "email": "john.doe@example.com"
  }'
```

### Using the .http files:

**Service Invocation:**
```http
GET {{DaprOneService_API_HostAddress}}/products
Accept: application/json
```

**Pub/Sub:**
```http
POST {{DaprOneService_API_HostAddress}}/users
Content-Type: application/json

{
  "userName": "John Doe",
  "email": "john.doe@example.com"
}
```

## What Happens

### Service Invocation Flow:
1. Client calls `GET /products` on DaprOneService
2. DaprOneService uses `DaprClient.InvokeMethodAsync()` to call DaprTwoService
3. Request goes through Dapr sidecars (service discovery, mTLS, retry)
4. DaprTwoService returns product list
5. Response flows back to the client

### Pub/Sub Flow:
1. Client calls `POST /users` on DaprOneService
2. DaprOneService creates a user and publishes a `UserCreatedEvent`
3. Event is sent to the Dapr sidecar via the pub/sub API
4. Dapr routes the event to all subscribers of the `user-created` topic
5. DaprTwoService receives the event at its `/user-created` endpoint
6. DaprTwoService logs the user information and processes the event

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

## Data Models

### Event Model
```csharp
public record UserCreatedEvent(
    Guid UserId,
    string UserName,
    string Email,
    DateTime CreatedAt
);
```

### Product Model
```csharp
public record Product(
    int Id, 
    string Name, 
    string Description, 
    decimal Price
);
```

## Key Features

- ✅ **Pub/Sub**: Decoupled asynchronous event-driven architecture
- ✅ **Service Invocation**: Synchronous service-to-service communication
- ✅ Service discovery (no hardcoded URLs/ports)
- ✅ Automatic retries and error handling
- ✅ mTLS for secure communication
- ✅ Distributed tracing and observability
- ✅ .NET Aspire integration for orchestration
- ✅ Shared event models in separate library
- ✅ Vendor-agnostic (switch components without code changes)
