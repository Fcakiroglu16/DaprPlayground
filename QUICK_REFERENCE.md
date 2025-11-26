# Quick Reference Guide

## 🚀 Start the Application

```bash
cd DaprPlayground.AppHost
dotnet run
```

## 📡 API Endpoints

### DaprOneService.API (http://localhost:5015)

| Method | Endpoint | Description | Type |
|--------|----------|-------------|------|
| GET | `/products` | Get products via Dapr service invocation | Service Invocation |
| POST | `/users` | Create user and publish event | Pub/Sub |

### DaprTwoService.API (http://localhost:5076)

| Method | Endpoint | Description | Type |
|--------|----------|-------------|------|
| GET | `/products` | Return sample products | Direct |
| POST | `/user-created` | Consume UserCreatedEvent | Pub/Sub Subscription |

## 🧪 Quick Tests

### Test 1: Service Invocation (Synchronous)
```bash
curl http://localhost:5015/products
```

**Expected:** List of 5 products (Laptop, Mouse, Keyboard, Monitor, Headphones)

### Test 2: Direct Call
```bash
curl http://localhost:5076/products
```

**Expected:** Same list of products (direct to service, bypassing Dapr invocation)

### Test 3: Pub/Sub (Asynchronous)
```bash
curl -X POST http://localhost:5015/users \
  -H "Content-Type: application/json" \
  -d '{"userName": "Jane Doe", "email": "jane@example.com"}'
```

**Expected:** 
- Response: `{"userId": "...", "message": "User created and event published"}`
- Check DaprTwoService logs for: `Received UserCreatedEvent: UserId=..., UserName=Jane Doe...`

## 🔧 Using .http Files (in IDE)

### DaprOneService.API.http
```http
### Get Products via Service Invocation
GET http://localhost:5015/products
Accept: application/json

###

### Create User (Pub/Sub)
POST http://localhost:5015/users
Content-Type: application/json

{
  "userName": "John Doe",
  "email": "john.doe@example.com"
}
```

### DaprTwoService.API.http
```http
### Get Products (Direct)
GET http://localhost:5076/products
Accept: application/json
```

## 📊 Response Examples

### GET /products
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
  }
]
```

### POST /users
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "User created and event published"
}
```

## 🔍 Verify Dapr is Running

```bash
# Check Dapr CLI version
dapr --version

# List running Dapr apps (after starting AppHost)
dapr list

# Open Dapr dashboard
dapr dashboard
```

## 📝 Log Monitoring

### View DaprTwoService Logs
When you publish a user creation event, you should see:
```
info: Program[0]
      Received UserCreatedEvent: UserId=3fa85f64-5717-4562-b3fc-2c963f66afa6, 
      UserName=Jane Doe, Email=jane@example.com, CreatedAt=2025-11-26T...
```

## 🛠️ Troubleshooting

### Issue: Connection refused
**Solution:** Make sure AppHost is running and services are started

### Issue: "No such host is known" for service invocation
**Solution:** Check that App IDs match in AppHost.cs (`daproneservice-api`, `daprtwoservice-api`)

### Issue: Events not received
**Solution:** 
1. Check Dapr sidecars are running: `dapr list`
2. Verify pubsub component exists: `ls DaprPlayground.AppHost/dapr/components/`
3. Check DaprTwoService logs for subscription confirmation

### Issue: Ports already in use
**Solution:** Check Properties/launchSettings.json and change ports if needed

## 📚 Documentation Files

- `README.md` - Main documentation
- `IMPLEMENTATION.md` - Implementation details
- `SERVICE_INVOCATION.md` - Service invocation deep dive
- `SETUP.md` - Prerequisites and setup
- `COMPLETED.md` - What was implemented (this session)

## 🎯 Communication Flow Summary

### Service Invocation Flow
```
Client → DaprOneService → Dapr Sidecar → Dapr Sidecar → DaprTwoService → Response
```

### Pub/Sub Flow
```
Client → DaprOneService → Dapr Sidecar → Pub/Sub Component → Dapr Sidecar → DaprTwoService
```

## ⚡ All-in-One Test Script

Run this to test everything:
```bash
./test-event.sh
```

## 🎉 Success Checklist

- [ ] AppHost running without errors
- [ ] Can get products from http://localhost:5015/products
- [ ] Can get products from http://localhost:5076/products
- [ ] Can create user and see event published
- [ ] Can see event consumed in DaprTwoService logs
- [ ] Dapr dashboard shows both services

---

**Ready to explore Dapr? Start with the Quick Tests above!** 🚀
