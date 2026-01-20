# Dapr Playground - Kubernetes Deployment Guide

This project demonstrates two key Dapr communication patterns between microservices running on Kubernetes:
1. **Pub/Sub** (Asynchronous event-based communication)
2. **Service Invocation** (Synchronous service-to-service calls)

## 📋 Table of Contents
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Deployment Steps](#deployment-steps)
  - [1. OpenTelemetry Setup](#1-opentelemetry-setup)
  - [2. RabbitMQ Setup](#2-rabbitmq-setup)
  - [3. Redis Setup](#3-redis-setup)
  - [4. Dapr Components Setup](#4-dapr-components-setup)
  - [5. DaprOneService Setup](#5-daprone-service-setup)
  - [6. DaprTwoService Setup](#6-daptwo-service-setup)
- [Testing the Application](#testing-the-application)
- [Communication Flow](#communication-flow)
- [Observability](#observability)
- [Cleanup](#cleanup)

## 🏗️ Architecture

### Services

**DaprOneService.API**
- Publishes `UserCreatedEvent` events via RabbitMQ (Pub/Sub)
- Calls DaprTwoService to get products (Service Invocation)
- Uses Redis for state management

**DaprTwoService.API**
- Subscribes to `UserCreatedEvent` events via RabbitMQ (Pub/Sub)
- Provides products endpoint (Service Invocation)
- Uses Redis for state management

**DaprPlayground.Events**
- Shared library containing event models

### Infrastructure Components

- **OpenTelemetry**: Distributed tracing and observability
- **Jaeger**: Trace visualization
- **RabbitMQ**: Pub/Sub message broker
- **Redis**: State store

## 📦 Prerequisites

- Kubernetes cluster (Docker Desktop, Minikube, or cloud provider)
- kubectl CLI installed and configured
- Dapr installed on Kubernetes cluster

### Install Dapr on Kubernetes

```bash
dapr init -k
```

Verify Dapr installation:

```bash
dapr status -k
```

Expected output:
```
NAME                   NAMESPACE    HEALTHY  STATUS   REPLICAS  VERSION  AGE  CREATED
dapr-sidecar-injector  dapr-system  True     Running  1         1.x.x    Xs   2024-01-21 00:00:00
dapr-sentry            dapr-system  True     Running  1         1.x.x    Xs   2024-01-21 00:00:00
dapr-operator          dapr-system  True     Running  1         1.x.x    Xs   2024-01-21 00:00:00
dapr-placement         dapr-system  True     Running  1         1.x.x    Xs   2024-01-21 00:00:00
```

## 🚀 Deployment Steps

### 1. OpenTelemetry Setup

OpenTelemetry provides distributed tracing and observability for microservices.

#### Deployment Command

```bash
kubectl apply -f k8s/opentelemetry/
```

#### What Gets Deployed?

- **OpenTelemetry Collector**: Trace collection with OTLP receiver
- **Jaeger**: Trace visualization and analysis
- **ConfigMap**: OpenTelemetry Collector configuration
- **Service**: 
  - Port 4317 (gRPC - OTLP)
  - Port 4318 (HTTP - OTLP)
  - Port 16686 (Jaeger UI)

#### Verification

```bash
# Check pods
kubectl get pods | findstr otel

# Check services
kubectl get svc otel-collector

# Check ConfigMap
kubectl get configmap otel-collector-config
```

Expected output:
```
otel-collector-xxxxxxxxx-xxxxx   1/1     Running   0          Xs
```

#### Access Jaeger UI (Optional)

```bash
kubectl port-forward svc/otel-collector 16686:16686
```

Open in browser: `http://localhost:16686`

---

### 2. RabbitMQ Setup

RabbitMQ serves as the message broker for Dapr Pub/Sub communication.

#### Deployment Command

```bash
kubectl apply -f k8s/rabbitmq/
```

#### What Gets Deployed?

- **RabbitMQ Deployment**: RabbitMQ 3 Management Alpine image
- **RabbitMQ Service**: 
  - Port 5672 (AMQP protocol)
  - Port 15672 (Management UI)
- **Default Credentials**: 
  - Username: `guest`
  - Password: `guest`

#### Verification

```bash
# Check pods
kubectl get pods | findstr rabbitmq

# Check services
kubectl get svc rabbitmq

# Check pod logs
kubectl logs -l app=rabbitmq
```

Expected output:
```
rabbitmq-xxxxxxxxx-xxxxx   1/1     Running   0          Xs
```

#### Access RabbitMQ Management UI

```bash
kubectl port-forward svc/rabbitmq 15672:15672
```

Open in browser: `http://localhost:15672`
- Username: `guest`
- Password: `guest`

From the Management UI you can view:
- Connections
- Channels
- Exchanges
- Queues
- Message rates

---

### 3. Redis Setup

Redis serves as the state store for Dapr State Management.

#### Deployment Command

```bash
kubectl apply -f k8s/redis/
```

#### What Gets Deployed?

- **Redis Deployment**: Redis 7 Alpine image
- **Redis Service**: Port 6379
- **Volume**: EmptyDir (temporary data storage)

#### Verification

```bash
# Check pods
kubectl get pods | findstr redis

# Check services
kubectl get svc redis

# Test Redis connection
kubectl run redis-test --rm -it --image=redis:7-alpine -- redis-cli -h redis ping
```

Expected output:
```
redis-xxxxxxxxx-xxxxx   1/1     Running   0          Xs
```

Redis connection test should return `PONG`.

---

### 4. Dapr Components Setup

Dapr components contain configurations for Pub/Sub and State Management.

#### Deployment Command

```bash
kubectl apply -f k8s/dapr/components/
```

#### What Gets Deployed?

**pubsub-rabbitmq.yaml**: RabbitMQ Pub/Sub component
- Component name: `pubsub`
- Type: `pubsub.rabbitmq`
- Host: `amqp://guest:guest@rabbitmq:5672`
- Durable: `true`
- Auto-ack: `false`

**statestore-redis.yaml**: Redis State Store component
- Component name: `statestore`
- Type: `state.redis`
- Host: `redis:6379`
- Actor state store: `true`

#### Verification

```bash
# List Dapr components
dapr components -k

# Or check with kubectl
kubectl get components
```

Expected output:
```
NAME         AGE
pubsub       Xs
statestore   Xs
```

---

### 5. DaprOne Service Setup

DaprOneService publishes events and calls other services.

#### Deployment Command

```bash
kubectl apply -f k8s/daprone-service/
```

#### What Gets Deployed?

- **DaprOneService Deployment**:
  - Container: `daproneserviceapi:latest`
  - Port: 8080
  - Environment variables (OTEL endpoint, ASPNETCORE settings)
  
- **Dapr Sidecar Annotations**:
  - `dapr.io/enabled: "true"`
  - `dapr.io/app-id: "daprone-service-api"`
  - `dapr.io/app-port: "8080"`
  - `dapr.io/config: "dapr-config"`

- **Service**:
  - Type: ClusterIP
  - Port: 80 → 8080

#### Verification

```bash
# Check pods (should have 2 containers: app + daprd sidecar)
kubectl get pods | findstr daprone

# View pod details
kubectl describe pod -l app=daprone-service-api

# Check service
kubectl get svc daprone-service-api

# View logs
kubectl logs -l app=daprone-service-api -c daprone-service-api
kubectl logs -l app=daprone-service-api -c daprd
```

Expected output:
```
daprone-service-api-xxxxxxxxx-xxxxx   2/2     Running   0          Xs
```

The `2/2` indicator shows both the application container and Dapr sidecar are running.

---

### 6. DaprTwo Service Setup

DaprTwoService subscribes to events and provides endpoints.

#### Deployment Command

```bash
kubectl apply -f k8s/daptwo-service/
```

#### What Gets Deployed?

- **DaprTwoService Deployment**:
  - Container: `daprtwoserviceapi:latest`
  - Port: 8080
  - Environment variables (OTEL endpoint, ASPNETCORE settings)
  
- **Dapr Sidecar Annotations**:
  - `dapr.io/enabled: "true"`
  - `dapr.io/app-id: "daptwo-service-api"`
  - `dapr.io/app-port: "8080"`
  - `dapr.io/config: "dapr-config"`

- **Service**:
  - Type: ClusterIP
  - Port: 80 → 8080

#### Verification

```bash
# Check pods (should have 2 containers: app + daprd sidecar)
kubectl get pods | findstr daptwo

# View pod details
kubectl describe pod -l app=daptwo-service-api

# Check service
kubectl get svc daptwo-service-api

# View logs
kubectl logs -l app=daptwo-service-api -c daptwo-service-api
kubectl logs -l app=daptwo-service-api -c daprd
```

Expected output:
```
daptwo-service-api-xxxxxxxxx-xxxxx   2/2     Running   0          Xs
```

---

## ✅ Verify All Deployments

Check all resources:

```bash
# List all resources
kubectl get all

# Check pod status
kubectl get pods

# Check services
kubectl get svc

# Check Dapr components
dapr components -k

# Check Dapr configurations
kubectl get configurations
```

Expected pods:
- ✅ otel-collector-xxxxx (1/1)
- ✅ rabbitmq-xxxxx (1/1)
- ✅ redis-xxxxx (1/1)
- ✅ daprone-service-api-xxxxx (2/2)
- ✅ daptwo-service-api-xxxxx (2/2)

---

## 🧪 Testing the Application

### Port Forwarding

Forward DaprOneService to your local machine:

```bash
kubectl port-forward svc/daprone-service-api 8080:80
```

### 1. Service Invocation Test (Get Products)

Get products via DaprOneService (internally calls DaprTwoService):

```bash
curl http://localhost:8080/products
```

Expected response:
```json
[
  {
    "id": 1,
    "name": "Product 1",
    "description": "Description 1",
    "price": 100.00
  },
  {
    "id": 2,
    "name": "Product 2",
    "description": "Description 2",
    "price": 200.00
  }
]
```

### 2. Pub/Sub Test (Create User)

Create a new user (publishes event to RabbitMQ):

```bash
curl -X POST http://localhost:8080/users -H "Content-Type: application/json" -d "{\"userName\": \"John Doe\", \"email\": \"john@example.com\"}"
```

Expected response:
```json
{
  "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "userName": "John Doe",
  "email": "john@example.com",
  "createdAt": "2026-01-21T00:00:00Z"
}
```

### 3. Verify Event Processing

Check that DaprTwoService received the event:

```bash
# Publisher logs (DaprOneService)
kubectl logs -l app=daprone-service-api -c daprone-service-api --tail=20

# Subscriber logs (DaprTwoService)
kubectl logs -l app=daptwo-service-api -c daptwo-service-api --tail=20
```

In DaprTwoService logs, you should see output similar to:
```
User created event received: John Doe (john@example.com)
```

---

## 🔄 Communication Flow

### Service Invocation Flow (Synchronous)

```
┌─────────┐                ┌──────────────────┐                ┌──────────────────┐
│ Client  │                │ DaprOneService   │                │ DaprTwoService   │
└────┬────┘                └────────┬─────────┘                └────────┬─────────┘
     │                              │                                   │
     │  GET /products               │                                   │
     ├─────────────────────────────>│                                   │
     │                              │                                   │
     │                              │  Dapr Service Invocation          │
     │                              ├──────────────────────────────────>│
     │                              │  (service discovery, mTLS)        │
     │                              │                                   │
     │                              │  GET /products                    │
     │                              │<──────────────────────────────────┤
     │                              │                                   │
     │  Product List                │                                   │
     │<─────────────────────────────┤                                   │
     │                              │                                   │
```

**Steps:**
1. Client calls `GET /products` on DaprOneService
2. DaprOneService uses `DaprClient.InvokeMethodAsync()` to call DaprTwoService
3. Request goes through Dapr sidecars (service discovery, mTLS, retry)
4. DaprTwoService returns product list
5. Response flows back to the client

### Pub/Sub Flow (Asynchronous)

```
┌─────────┐          ┌──────────────────┐          ┌──────────┐          ┌──────────────────┐
│ Client  │          │ DaprOneService   │          │ RabbitMQ │          │ DaprTwoService   │
└────┬────┘          └────────┬─────────┘          └─────┬────┘          └────────┬─────────┘
     │                        │                          │                        │
     │  POST /users           │                          │                        │
     ├───────────────────────>│                          │                        │
     │                        │                          │                        │
     │                        │  Publish UserCreatedEvent│                        │
     │                        ├─────────────────────────>│                        │
     │                        │  (via Dapr Pub/Sub)      │                        │
     │                        │                          │                        │
     │  201 Created           │                          │  Subscribe to topic    │
     │<───────────────────────┤                          ├───────────────────────>│
     │                        │                          │                        │
     │                        │                          │  UserCreatedEvent      │
     │                        │                          │  (via Dapr sidecar)    │
     │                        │                          │                        │
     │                        │                          │  Process Event         │
     │                        │                          │  (Log user info)       │
     │                        │                          │<───────────────────────┤
```

**Steps:**
1. Client calls `POST /users` on DaprOneService
2. DaprOneService creates a user and publishes a `UserCreatedEvent`
3. Event is sent to RabbitMQ via Dapr Pub/Sub API
4. Dapr routes the event to all subscribers of the `user-created` topic
5. DaprTwoService receives the event at its `/user-created` endpoint
6. DaprTwoService logs user information and processes the event

---

## 📊 Observability

### View Logs

#### DaprOneService Logs

```bash
# Application logs
kubectl logs -l app=daprone-service-api -c daprone-service-api --tail=50 -f

# Dapr sidecar logs
kubectl logs -l app=daprone-service-api -c daprd --tail=50 -f
```

#### DaprTwoService Logs

```bash
# Application logs
kubectl logs -l app=daptwo-service-api -c daptwo-service-api --tail=50 -f

# Dapr sidecar logs
kubectl logs -l app=daptwo-service-api -c daprd --tail=50 -f
```

#### Infrastructure Logs

```bash
# RabbitMQ logs
kubectl logs -l app=rabbitmq --tail=50 -f

# Redis logs
kubectl logs -l app=redis --tail=50 -f

# OpenTelemetry Collector logs
kubectl logs -l app=otel-collector --tail=50 -f
```

### Distributed Tracing

OpenTelemetry Collector sends traces to Jaeger.

#### Access Jaeger UI

```bash
kubectl port-forward svc/otel-collector 16686:16686
```

Open in browser: `http://localhost:16686`

#### What You Can See in Jaeger

- **Service Graph**: Dependencies between services
- **Traces**: Detailed timeline of each request
- **Spans**: Duration of each operation
- **Dependencies**: Call graph between services
- **Errors**: Error rates and details

#### Example Traces

1. **Service Invocation Trace**: `GET /products` request flow from DaprOneService → DaprTwoService
2. **Pub/Sub Trace**: `UserCreatedEvent` event flow from publish → consume

---

## 🧹 Cleanup

To remove all resources:

```bash
# Remove services
kubectl delete -f k8s/daptwo-service/
kubectl delete -f k8s/daprone-service/

# Remove Dapr components
kubectl delete -f k8s/dapr/components/

# Remove infrastructure components
kubectl delete -f k8s/redis/
kubectl delete -f k8s/rabbitmq/
kubectl delete -f k8s/opentelemetry/
```

Verify all resources are removed:

```bash
kubectl get all
kubectl get components
kubectl get configmaps
```

---

## 📚 Data Models

### Event Model

```csharp
namespace DaprPlayground.Events;

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

---

## ✨ Key Features

- ✅ **Pub/Sub Pattern**: RabbitMQ-based asynchronous event-driven architecture
- ✅ **Service Invocation**: Synchronous service-to-service communication
- ✅ **State Management**: Redis-based distributed state store
- ✅ **Service Discovery**: Automatic discovery via Kubernetes service names
- ✅ **Automatic Retries**: Automatic retry and error handling via Dapr
- ✅ **mTLS**: Secure communication between services
- ✅ **Distributed Tracing**: OpenTelemetry integration for distributed tracing
- ✅ **Scalability**: Production-ready scalable deployment
- ✅ **Vendor Agnostic**: Switch components without code changes

---

## 🔧 Kubernetes Folder Structure

```
k8s/
├── opentelemetry/              # OpenTelemetry and Jaeger configurations
│   ├── otel-collector-config.yaml
│   ├── otel-collector-deployment.yaml
│   ├── otel-collector-service.yaml
│   └── jaeger-deployment.yaml
├── rabbitmq/                   # RabbitMQ deployment and service
│   ├── rabbitmq-deployment.yaml
│   └── rabbitmq-pvc.yaml
├── redis/                      # Redis deployment and service
│   ├── redis-deployment.yaml
│   └── redis-service.yaml
├── dapr/
│   └── components/             # Dapr component configurations
│       ├── pubsub-rabbitmq.yaml
│       └── statestore-redis.yaml
├── daprone-service/            # DaprOneService deployment
│   └── daprone-service-api-deployment.yaml
└── daptwo-service/             # DaprTwoService deployment
    └── daptwo-service-api-deployment.yaml
```

---

## 📞 Troubleshooting

### Pods Not Starting

```bash
# Check pod status
kubectl describe pod <pod-name>

# View pod logs
kubectl logs <pod-name> -c <container-name>
```

### Dapr Sidecar Not Injected

```bash
# Verify Dapr is running
dapr status -k

# Check namespace Dapr annotations
kubectl get namespace default -o yaml
```

### Cannot Connect to RabbitMQ

```bash
# Check RabbitMQ service
kubectl get svc rabbitmq

# Connect to RabbitMQ pod
kubectl exec -it <rabbitmq-pod> -- rabbitmqctl status
```

### Cannot Connect to Redis

```bash
# Check Redis service
kubectl get svc redis

# Test Redis connection
kubectl run redis-test --rm -it --image=redis:7-alpine -- redis-cli -h redis ping
```

---

## 📖 Learn More

- [Dapr Documentation](https://docs.dapr.io/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Redis Documentation](https://redis.io/documentation)

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
