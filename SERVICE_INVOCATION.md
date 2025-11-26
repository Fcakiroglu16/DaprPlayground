# Dapr Service Invocation - Products Example

## Overview

This demonstrates **Dapr service-to-service invocation** where DaprOneService.API calls DaprTwoService.API to retrieve products.

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     User/Client                         │
└───────────────────────┬─────────────────────────────────┘
                        │
                        │ GET /products
                        ▼
            ┌───────────────────────────┐
            │  DaprOneService.API       │
            │  (Consumer/Gateway)       │
            └───────────┬───────────────┘
                        │
                        │ Dapr Service Invocation
                        │ InvokeMethodAsync()
                        ▼
            ┌───────────────────────────┐
            │  Dapr Sidecar             │
            │  (daproneservice-api)     │
            └───────────┬───────────────┘
                        │
                        │ Service Discovery
                        │ Load Balancing
                        │ Retry & Resilience
                        ▼
            ┌───────────────────────────┐
            │  Dapr Sidecar             │
            │  (daprtwoservice-api)     │
            └───────────┬───────────────┘
                        │
                        ▼
            ┌───────────────────────────┐
            │  DaprTwoService.API       │
            │  (Product Provider)       │
            │                           │
            │  GET /products            │
            │  Returns sample products  │
            └───────────────────────────┘
```

## Endpoints

### DaprTwoService.API (Provider)

**GET /products**
- Returns a list of sample products
- Direct endpoint that can be called via Dapr

**Response:**
```json
[
  {
    "id": 1,
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99
  },
  {
    "id": 2,
    "name": "Mouse",
    "description": "Wireless ergonomic mouse",
    "price": 29.99
  },
  ...
]
```

### DaprOneService.API (Consumer)

**GET /products**
- Calls DaprTwoService via Dapr service invocation
- Uses `DaprClient.InvokeMethodAsync()`
- Returns the same product list

## How It Works

### In DaprTwoService.API:
```csharp
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
```

### In DaprOneService.API:
```csharp
app.MapGet("/products", async (DaprClient daprClient) =>
{
    var products = await daprClient.InvokeMethodAsync<Product[]>(
        HttpMethod.Get,
        "daprtwoservice-api",  // App ID of the target service
        "products");            // Endpoint path
    
    return Results.Ok(products);
}).WithName("GetProductsFromServiceTwo");
```

## Testing

### Option 1: Direct call to DaprTwoService
```bash
curl http://localhost:5076/products
```

### Option 2: Call via DaprOneService (using Dapr service invocation)
```bash
curl http://localhost:5015/products
```

### Option 3: Use .http files

**DaprOneService.API.http:**
```http
GET {{DaprOneService.API_HostAddress}}/products
Accept: application/json
```

**DaprTwoService.API.http:**
```http
GET {{DaprTwoService.API_HostAddress}}/products
Accept: application/json
```

## Benefits of Dapr Service Invocation

1. **Service Discovery**: No need to know the exact URL/port of DaprTwoService
2. **Load Balancing**: Dapr can distribute requests across multiple instances
3. **Retry & Resilience**: Built-in retry policies and circuit breakers
4. **mTLS**: Automatic mutual TLS for secure communication
5. **Observability**: Distributed tracing and metrics out of the box
6. **Platform Agnostic**: Works across different hosting environments (Kubernetes, VMs, etc.)

## Communication Patterns in This Solution

### 1. **Service Invocation** (Synchronous)
- **Use Case**: Get products
- **Pattern**: Request/Response
- **DaprOneService → DaprTwoService**
- **Method**: `DaprClient.InvokeMethodAsync()`

### 2. **Pub/Sub** (Asynchronous)
- **Use Case**: User created events
- **Pattern**: Fire and Forget / Event-Driven
- **DaprOneService → DaprTwoService**
- **Method**: `DaprClient.PublishEventAsync()`

## Key Differences

| Feature | Service Invocation | Pub/Sub |
|---------|-------------------|---------|
| **Communication** | Synchronous | Asynchronous |
| **Response** | Returns data | Fire and forget |
| **Coupling** | Tighter (knows target) | Loose (topic-based) |
| **Use Case** | Query data | Event notifications |
| **Example** | Get products | User created |

## App IDs

The App ID is crucial for service invocation. It's set in the AppHost:

```csharp
builder.AddProject<Projects.DaprOneService_API>("daproneservice-api")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "daproneservice-api",  // This is the App ID
        ResourcesPaths = [componentsPath]
    });
```

When calling a service, use its App ID:
```csharp
await daprClient.InvokeMethodAsync<Product[]>(
    HttpMethod.Get,
    "daprtwoservice-api",  // ← App ID of target service
    "products");
```

## Summary

This implementation demonstrates both **synchronous** (service invocation) and **asynchronous** (pub/sub) communication patterns using Dapr:

- ✅ **GET /products**: Synchronous service-to-service call
- ✅ **POST /users**: Asynchronous event publishing
- ✅ **UserCreatedEvent**: Event subscription and handling

Both patterns work seamlessly through Dapr sidecars without the services needing to know each other's network details!
