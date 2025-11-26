# Prerequisites and Setup Guide

## Required Software

### 1. Dapr CLI
You need to install Dapr CLI to run the application with Dapr sidecars.

**Installation:**
```bash
# macOS
brew install dapr/tap/dapr-cli

# Or using the installation script
curl -fsSL https://raw.githubusercontent.com/dapr/cli/master/install/install.sh | /bin/bash
```

**Verify installation:**
```bash
dapr --version
```

### 2. Initialize Dapr
After installing Dapr CLI, initialize Dapr in your local environment:

```bash
dapr init
```

This will:
- Install Dapr runtime
- Set up Redis and Zipkin containers (using Docker)
- Configure default components

**Verify Dapr is running:**
```bash
dapr --version
docker ps  # You should see dapr_redis and dapr_zipkin containers
```

### 3. .NET SDK
Make sure you have .NET 9.0 or .NET 10.0 SDK installed:

```bash
dotnet --version
```

## Quick Start

### Step 1: Install Dapr (if not already installed)
```bash
# Install Dapr CLI
brew install dapr/tap/dapr-cli

# Initialize Dapr
dapr init
```

### Step 2: Build the Solution
```bash
cd /Users/fatih-mac/RiderProjects/DaprPlayground
dotnet build
```

### Step 3: Run the Application
```bash
cd DaprPlayground.AppHost
dotnet run
```

The .NET Aspire AppHost will automatically:
- Start both services
- Launch Dapr sidecars for each service
- Configure the pub/sub component
- Set up service discovery

### Step 4: Test the Event Flow
```bash
# From the project root
./test-event.sh

# Or manually:
curl -X POST http://localhost:5015/users \
  -H "Content-Type: application/json" \
  -d '{"userName": "Jane Smith", "email": "jane@example.com"}'
```

### Step 5: Verify Event Reception
Check the console output of DaprTwoService.API. You should see:
```
info: Program[0]
      Received UserCreatedEvent: UserId=xxx, UserName=Jane Smith, Email=jane@example.com, CreatedAt=...
```

## Troubleshooting

### Issue: "dapr: command not found"
**Solution:** Install Dapr CLI (see Step 1 above)

### Issue: Dapr sidecar fails to start
**Solution:** Make sure Dapr is initialized:
```bash
dapr init
```

### Issue: Events not being received
**Checklist:**
1. Both services are running
2. Dapr sidecars are attached to both services
3. Check the Dapr dashboard: `dapr dashboard`
4. Verify component configuration in `dapr/components/pubsub.yaml`

### Issue: Port conflicts
**Solution:** Check `launchSettings.json` in each service's Properties folder and adjust ports if needed

## Dapr Dashboard

To monitor your Dapr applications:
```bash
dapr dashboard
```

Then open http://localhost:8080 in your browser to see:
- Running applications
- Components
- Pub/Sub subscriptions
- Distributed tracing

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    .NET Aspire AppHost                      │
│  (Orchestrates services and Dapr sidecars)                  │
└─────────────────────────────────────────────────────────────┘
                           │
           ┌───────────────┴───────────────┐
           │                               │
           ▼                               ▼
┌──────────────────────┐       ┌──────────────────────┐
│ DaprOneService.API   │       │ DaprTwoService.API   │
│  (Publisher)         │       │  (Subscriber)        │
│                      │       │                      │
│  POST /users         │       │  POST /user-created  │
│    │                 │       │    ▲                 │
│    │ Publish Event   │       │    │ Receive Event  │
└────┼─────────────────┘       └────┼─────────────────┘
     │                              │
     ▼                              ▼
┌──────────────────────┐       ┌──────────────────────┐
│ Dapr Sidecar         │       │ Dapr Sidecar         │
│  (App: daproneservice│       │  (App: daprtwoservice│
│         -api)        │       │         -api)        │
└──────────────────────┘       └──────────────────────┘
           │                               │
           └───────────────┬───────────────┘
                           ▼
                  ┌─────────────────┐
                  │   Pub/Sub       │
                  │  (in-memory)    │
                  │                 │
                  │  Topic:         │
                  │  user-created   │
                  └─────────────────┘
```

## Next Steps

1. ✅ Run the application
2. ✅ Test event publishing
3. ✅ Verify event consumption
4. 🔄 Explore Dapr dashboard
5. 🔄 Try different pub/sub providers (Redis, Azure Service Bus)
6. 🔄 Add more subscribers
7. 🔄 Implement additional event types
