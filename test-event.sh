#!/bin/bash

# Test script for Dapr event-based communication

echo "Testing Dapr Event-Based Communication"
echo "======================================="
echo ""

# Wait for user to confirm services are running
read -p "Make sure the AppHost is running. Press Enter to continue..."

echo ""
echo "Creating a user and publishing UserCreatedEvent..."
echo ""

# Send POST request to create user
response=$(curl -s -X POST http://localhost:5015/users \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "John Doe",
    "email": "john.doe@example.com"
  }')

echo "Response from DaprOneService:"
echo "$response"
echo ""
echo "Check the logs of DaprTwoService to see the consumed event!"
echo ""
echo "You should see a log message like:"
echo "Received UserCreatedEvent: UserId=..., UserName=John Doe, Email=john.doe@example.com, CreatedAt=..."
