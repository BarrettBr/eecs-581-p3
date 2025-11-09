/*
Prologue

Authors: Barrett Brown, Adam Berry, Alex Phibbs, Minh Vu, Jonathan Gott
Creation Date: 11/08/2025

Description:
- Defines ClientInfo (connection data) and the WebSocketHandler that manages a single WebSocket session lifecycle.
- Validates parameters ('roomID', optional 'game' *needed to set a game but not needed to create a room as we have a fallback)
  - Upgrades the HTTP request to a WebSocket, and registers the client.
- Reads frames (supports chunked messages), forwards text messages to the RoomHandler, and performs cleanup on disconnect.

Types:
- ClientInfo: Stores ClientID, RoomID, and the connected WebSocket

Functions:
- HandleWebSocket(context: HttpContext): Task
  Input:
    - HttpContext containing the WebSocket upgrade request, query params:
      - roomID: Guid (required) *Frontend automatically develops and sends one back so if using the frontend you are fine if using something like Postman it will throwback an error
      - game: string (optional; defaults from path or currently to "tictactoe")
  Behavior:
    - Validates roomID; returns 400 if missing/invalid
    - Accepts WebSocket, creates a ClientInfo, registers it, and joins/creates a room
    - Reads incoming frames until EndOfMessage or Close; on text frames:
        It decodes it as UTF-8 and forwards to RoomHandler.HandleStateAsync
    - On close/error: closes socket, leaves room, removes connection
  Output:
    - Nothing outside a potential error code if missing roomID; Side-effects can come from room membership changes and messages forwarded to RoomHandler

Inputs:
- WebSocket text messages representing game state updates from the client

Outputs:
- None directly; messages are forwarded to RoomHandler which broadcasts to clients
- Proper WebSocket close handshake and connection cleanup
*/

namespace SocketHandler.Core
{
  using Microsoft.AspNetCore.Http;
  using System.Net.WebSockets;
  using System.Collections.Concurrent;
  using Microsoft.AspNetCore.Mvc.ModelBinding;

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

      // Parse information from request
      string requestRoute = context.Request.Path.ToString(); // The full requestRoute/token bit not the value, for the moment not used but maybe later if we need it.
      var path = context.Request.Path.Value ?? "/"; // Null coalesing operator if left is null return just a / symbol
      var seg = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries); // Remove front/end / symbol then split the remainder based on / symbols so now we get a bit like {"tictactoe", "roomID"} and so on
      var gameParam = context.Request.Query["game"]; // Since it is from a /ws bit we parse out the game
      var gameKey = !string.IsNullOrWhiteSpace(gameParam)
                    ? gameParam.ToString().ToLowerInvariant() // If gameParam is real use it
                    : (seg.Length > 0 ? seg[0].ToLowerInvariant() : "tictactoe"); // Else default to tictactoe TODO: Might remove/redirect instead of default


      var quickParam = context.Request.Query["quickPlayJoin"].ToString();
      bool quickPlay = quickParam.Equals("true", StringComparison.OrdinalIgnoreCase);

      // Get the underlying socket & generate a new GUID for the conneciton
      using var socket = await context.WebSockets.AcceptWebSocketAsync();


      // Create client object based on currently connected user
      var clientID = Guid.NewGuid();
      var client = new ClientInfo
      {
        ClientID = clientID,
        RoomID = Guid.Empty,
        Socket = socket,
      };
      Connections[clientID] = client;
      if (quickPlay)
      {
        // Requested quickplay
        var (joined, roomId, joinedGameKey) = RoomHandler.QuickPlay(client);

        if (!joined)
        {
          // No rooms Free send to frontend for reactive response
          var payload = System.Text.Json.JsonSerializer.Serialize(new
          {
            Event = "nofree"
          });
          var buffer = System.Text.Encoding.UTF8.GetBytes(payload);
          await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        else
        {
          // Joined room send frontend new roomId + key so they can redirect
          var payload = System.Text.Json.JsonSerializer.Serialize(new
          {
            Event = "quickPlayJoined",
            RoomID = roomId,
            GameKey = joinedGameKey
          });
          var buffer = System.Text.Encoding.UTF8.GetBytes(payload);
          await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
      }
      else
      {
        // Change token to value of room id from URL (?roomID=value)
        // Convert token from string -> Guid
        var roomIDToken = context.Request.Query["roomID"];
        if (!Guid.TryParse(roomIDToken, out var roomID))
        {
          var error = System.Text.Json.JsonSerializer.Serialize(new { Event = "error", Message = "Missing or invalid roomID" });
          var buffer = System.Text.Encoding.UTF8.GetBytes(error);
          await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
          await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid roomID", CancellationToken.None);
          Connections.TryRemove(clientID, out _);
          return;
        }
        client.RoomID = roomID;
        RoomHandler.JoinOrCreateRoom(roomID, gameKey, client);
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
          await RoomHandler.HandleStateAsync(client, message);
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
