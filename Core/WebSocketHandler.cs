namespace SocketHandler.Core
{
  using Microsoft.AspNetCore.Http;
  using System.Net.WebSockets;
  using System.Collections.Concurrent;
  public sealed class ClientInfo
  {
    public required Guid ClientId { get; set; }
    public required Guid RoomId { get; set; }
    public required WebSocket Socket { get; init; }
  }
  public class WebSocketHandler
  {
    private readonly ConcurrentDictionary<Guid, ClientInfo> connDict = new();
    public static async Task HandleWebSocket(HttpContext context)
    {
      // Get the underlying socket & generate a new GUID for the conneciton
      using var socket = await context.WebSockets.AcceptWebSocketAsync();

      // 1. Extract useful information from HttpContext www.website.com/requestRoute/?token=value
      string requestRoute = context.Request.Path.ToString();
      string? token = context.Request.Query["roomID"]; // Change token to token of room id from URL

      if (string.IsNullOrEmpty(token))
      {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Missing roomId token");
        return;
      }

      // Initialize containers for reading
      bool connectionAlive = true;
      var webSocketPayload = new List<byte>(1024 * 4); // 4 KB initial capacity (Shouldn't need more but can increase if needed)
      var tempMessage = new byte[1024 * 4]; // Message reader

      // 2. Connection loop
      while (connectionAlive && socket.State == WebSocketState.Open)
      {
        // Empty the container
        webSocketPayload.Clear();

        // Message handler
        WebSocketReceiveResult? webSocketResponse;

        // Read message in a loop until fully read (as message could be sent in chunks)
        do
        {
          // Wait until client sends message
          webSocketResponse = await socket.ReceiveAsync(tempMessage, CancellationToken.None);

          // Early exit check
          if (webSocketResponse.MessageType == WebSocketMessageType.Close) { connectionAlive = false; break; }

          // Save bytes
          webSocketPayload.AddRange(new ArraySegment<byte>(tempMessage, 0, webSocketResponse.Count));
        }
        while (!webSocketResponse.EndOfMessage);

        if (!connectionAlive) break;

        // Process Message (Will expand on this to pass/handle state updates)
        if (webSocketResponse.MessageType == WebSocketMessageType.Text)
        {
          // 3. Convert textual message from bytes to string
          string message = System.Text.Encoding.UTF8.GetString(webSocketPayload.ToArray());

          Console.WriteLine("Client says {0}", message);
        }
      }
      
      if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
      {
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
      }
      Console.WriteLine("Client Disconnected");
    }
  }
}
