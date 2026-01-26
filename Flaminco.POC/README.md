# Redis Pub/Sub POC

This is a proof of concept demonstrating Redis Pub/Sub with Server-Sent Events (SSE).

## How to Test

1. **Start the application:**
   ```bash
   dotnet run
   ```

2. **Open SSE endpoint in browser or use a tool that supports SSE:**
   ```
   http://localhost:5149/test-channel/events
   ```

3. **Publish a message using the HTTP endpoint:**
   ```bash
   POST http://localhost:5149/test-channel/publish
   Content-Type: application/json
   
   "Hello from HTTP client!"
   ```

## Endpoints

- `POST /{channelName}/publish` - Publishes a message to the Redis pub/sub channel
- `GET /{channelName}/events` - Opens an SSE connection to receive messages from the channel

## Testing with curl

```bash
# Terminal 1: Connect to SSE endpoint
curl -N http://localhost:5149/test-channel/events

# Terminal 2: Publish a message
curl -X POST http://localhost:5149/test-channel/publish \
  -H "Content-Type: application/json" \
  -d '"Hello World!"'
```

## Notes

- Make sure Redis is running on `localhost:6379`
- Pub/Sub messages are fire-and-forget (not persisted)
- If you publish before subscribing, the message will be lost
- Multiple SSE connections to the same channel will all receive the same messages
