namespace SocketHandler.Core
{
  using Microsoft.AspNetCore.Http;
  using System.Net.WebSockets;
  using System.Collections.Concurrent;
  public sealed class ClientInfo
  {
    // Description: Class that stores information about a client including their id, sokcet and the room they belong to.
    public required Guid ClientID { get; set; }
    public required Guid RoomID { get; set; }
    public required WebSocket Socket { get; init; }
  }
  public class WebSocketHandler
  {
    private static readonly ConcurrentDictionary<Guid, ClientInfo> Connections = new();
    public static async Task HandleWebSocket(HttpContext context)
    {
      // General form based off of medium article https://medium.com/@shtef21/how-to-create-a-web-socket-server-in-c-ea02eb9475cd
        // With some general adjustments for flow and expanding for error handling
      // Get the underlying socket & generate a new GUID for the conneciton
      using var socket = await context.WebSockets.AcceptWebSocketAsync();

      // 1. Extract useful information from HttpContext www.website.com/requestRoute/roomID?=value
      string requestRoute = context.Request.Path.ToString(); // The full requestRoute/token bit not the value
      var token = context.Request.Query["roomID"]; // Change token to value of room id from URL (roomID?=value)
      var path = context.Request.Path.Value ?? "/"; // Null coalesing operator if left is null return just a / symbol
      var seg = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries); // Remove front/end / symbol then split the remainder based on / symbols so now we get a bit like {"tictactoe", "roomID"} and so on
      var gameKey = seg.Length > 0 ? seg[0].ToLowerInvariant() : "tictactoe"; // TODO: Might change later to not be defaulted to tictactoe just done for now just do it for non-crash reasons; If a segment exists use the first which will be the game name otherwise use a default of tictactoe


      // Convert token from string -> Guid
      if (!Guid.TryParse(token, out var roomID))
      {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Missing roomID token");
        return;
      }

      // Create client object based on currently connected user
      var clientID = Guid.NewGuid();
      var client = new ClientInfo
      {
        ClientID = clientID,
        RoomID = roomID,
        Socket = socket,
      };
      Connections[clientID] = client;
      RoomHandler.JoinOrCreateRoom(roomID, gameKey, client);

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
          RoomHandler.HandleState(client, message);
          Console.WriteLine($"Client says {message}"); // TODO: Clean up or log post finishing up project; Debugging message of state from frontend
        }
      }

      // Final cleanup of client connection
      if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
      {
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
      }
      RoomHandler.LeaveRoom(client);
      Connections.TryRemove(clientID, out _);
      Console.WriteLine("Client Disconnected");
    }
  }
}
