#!/bin/bash

# Test script for Dapr communication (pub/sub and service invocation)

echo "Testing Dapr Communication Patterns"
echo "===================================="
echo ""

# Wait for user to confirm services are running
read -p "Make sure the AppHost is running. Press Enter to continue..."

echo ""
echo "1. Testing Service Invocation (Synchronous)"
echo "--------------------------------------------"
echo "Getting products from DaprTwoService via DaprOneService..."
echo ""

products_response=$(curl -s http://localhost:5015/products)
echo "Response:"
echo "$products_response" | jq '.' 2>/dev/null || echo "$products_response"

echo ""
echo ""
echo "2. Testing Pub/Sub (Asynchronous)"
echo "---------------------------------"
echo "Creating a user and publishing UserCreatedEvent..."
echo ""

# Send POST request to create user
user_response=$(curl -s -X POST http://localhost:5015/users \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "John Doe",
    "email": "john.doe@example.com"
  }')

echo "Response from DaprOneService:"
echo "$user_response" | jq '.' 2>/dev/null || echo "$user_response"
echo ""
echo "Check the logs of DaprTwoService to see the consumed event!"
echo ""
echo "You should see a log message like:"
echo "Received UserCreatedEvent: UserId=..., UserName=John Doe, Email=john.doe@example.com, CreatedAt=..."
echo ""
echo ""
echo "Summary:"
echo "--------"
echo "✅ Service Invocation: DaprOneService → DaprTwoService (GET /products)"
echo "✅ Pub/Sub: DaprOneService → DaprTwoService (UserCreatedEvent)"
