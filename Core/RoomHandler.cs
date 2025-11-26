using System.Collections.Concurrent;
using Game.Core;
using Microsoft.AspNetCore.StaticAssets;
using Microsoft.VisualBasic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Collections;
using Newtonsoft.Json;


/*
Prologue

Authors: Barrett Brown
Creation Date: 11/08/2025

Description:
- Central room manager for multiplayer sessions.
- Tracks active rooms and their clients, applies game state updates via the GameHandler, and broadcasts view updates.
- Provides helpers to find rooms, join/leave rooms, and send targeted or broadcast room-wide messages.

Types:
- Room: Represents a single room with a RoomID, Game (GameHandler), a RoomLock to ensure messages are sent in proper order, and a client map.

Functions:
- Instance: RoomHandler (singleton pattern)
- FindRoomByClientID(clientID: Guid): Room?
- FindRoomByRoomID(roomID: Guid): Room?
- HandleStateAsync(client: ClientInfo, state: string): Task
  - Locks the room, applies Game.Play(state, client), and if changed broadcasts the new View
- BroadcastView(view: object, room: Room): Task
  - Serializes '{ message: "view", value: view }' and sends to all clients in the room
- SendBoardToClient(view: object, client: ClientInfo): Task
  - Sends a single board/view update to one client (used on join)
- JoinOrCreateRoom(roomID: Guid, gameKey: string, client: ClientInfo): void
  - Creates room if missing (via GameFactory.CreateGame), adds client, sends current View, and calls Game.Join
- LeaveRoom(client: ClientInfo): void
  - Removes client from room; deletes room if it becomes empty

Inputs:
- Client join/leave events
- Per-client state messages (JSON/string) received from WebSocketHandler
- gameKey indicating which GameHandler to initiate for a new room

Outputs:
- Serialized view/board updates broadcast to all clients or a specific client
- Room lifecycle changes (create, add client, remove client, delete when empty)
*/


namespace SocketHandler.Core
{
  public sealed class RoomHandler
  {
    // Room instance
    private static RoomHandler? instance = null;
    // Lock
    private static readonly object padlock = new(); // Lock used for making the roomhandler instance work properly
    private static readonly ConcurrentDictionary<Guid, Room> rooms = new(); // <RoomID, Room> Used to store a list of all rooms for easy calling upon

    // Empty Class Constructor to prevent the class from being declared as an object without calling RoomHandler.Instance
    RoomHandler() { }

    public static RoomHandler Instance
    {
      // Sets the getter for roomhandlers instance to be based on the singleton design pattern so as to force 1 shared state between all calls.
      get
      {
        lock (padlock)
        {
          instance ??= new RoomHandler(); // Null coalescing operator. Used to simplify if (instance == null) {instance = new RoomHandler();} statement
          return instance;
        }
      }
    }

    public static Room? FindRoomByClientID(Guid clientID)
    {
      // Helper function used to get the room that a client is connected to
      foreach (var room in rooms.Values)
      {
        if (room.Clients.ContainsKey(clientID))
        {
          return room;
        }
      }
      return null;
    }

    public static Room? FindRoomByRoomID(Guid roomID)
    {
      // Quick helper function that takes in a RoomID and returns the room that corresponds to it
      return rooms.GetValueOrDefault(roomID);
    }
    public static string? ParseEvent(string state)
    {
      try
      {
        var msg = Newtonsoft.Json.Linq.JObject.Parse(state);
        return (string?)msg["Event"];
      }
      catch
      {
        return null;
      }
    }
    public static async Task HandleStateAsync(ClientInfo client, string state)
    {
      // Description: Takes a state from the frontend and pass to the GameHandler to handle/deal with
      // Inputs: Takes in the state update from from the frontend and passes to the backend gamehandler to run it on it's current board
      // Outputs: Doesn't return a value, instead it will let the gamehandler play method handle the state afterwards it passes off the new view to BroadcastView to send it out to all sockets
      var room = FindRoomByRoomID(client.RoomID);
      if (room == null)
      {
        Console.WriteLine($"Room Not found for client {client.ClientID}");
        return;
      }
      try
      {
        // Added lock to prevent 2 users from playing at the same time
        var event_state = ParseEvent(state)?.ToLowerInvariant();
        var alias = event_state == "move" ? Newtonsoft.Json.Linq.JObject.Parse(state)?["Alias"]?.ToString() : null;
        switch (event_state)
        {
          case "move":
            // Currently used in turn-based games as a "move, broadcast, wait, move" turn
            bool changed;
            lock (room.RoomLock)
            {
              changed = room.Game.Play(state, client);
            }
            if (changed)
            {
              await BroadcastView(room, "view", client);
              // Put if inside to handle accidentally doing more than 1 win per game
              if (room.Game.state == State.Win && alias != null){
                await DatabaseHandler.Instance.UpdateWin(room, alias);
              }
            }
            return;
          case "room.lock":
            // Event hit upon trying to lock the room, used to prvent quickplay joins
            if (!room.Game.Players.TryGetValue(client.ClientID, out var idx) || idx != 0)
            {
              return;
            }
            try
            {
              var msg = Newtonsoft.Json.Linq.JObject.Parse(state);
              var locked = (bool?)msg["locked"];
              if (locked != null)
              {
                room.QuickPlayLocked = locked.Value;
                await BroadcastView(room, "room.locked", client);
              }
            } catch {}
            return;
          case "chat":
            // Created base sending of a chat from Room -> clients
            try
            {
              // Parse out the chat from the json state and then 
              var chat = Newtonsoft.Json.Linq.JObject.Parse(state);
              var msg = (string?)chat["text"] ?? "";
              await BroadcastView(room, "chat", client, msg);
            }
            catch (Exception ex)
            {
              Console.WriteLine("Error Occured while sending chat:", ex);
            }
            return;
          default:
            // Default case that is hit upon not having an event defined (Null events go here)
            // Currently a copy of the "move" state might want to change this but for now we handle both the same
            bool ChangedDefault;
            lock (room.RoomLock)
            {
              ChangedDefault = room.Game.Play(state, client);
            }
            if (ChangedDefault)
            {
              await BroadcastView(room, "view", client);
              if (room.Game.state == State.Win && alias != null){
                await DatabaseHandler.Instance.UpdateWin(room, alias);
              }
            }
            return;
        }
      }
      catch (Exception e)
      {
        Console.WriteLine($"Error handling state {e.Message}");
      }
    }

    public static async Task BroadcastView(Room room, string eventType, ClientInfo client, object? state = null)
    {
      room.Game.Players.TryGetValue(client.ClientID, out int sender);
      object payload = eventType switch
      {
          "view" => new
          {
              Event = "view",
              Value = room.Game.View,
              State = room.Game.state
          },

          "room.locked" => new
          {
              Event = "room.locked",
              locked = room.QuickPlayLocked
          },
          "chat" => new
          {
            Event = "chat",
            Chat = state,
            From = sender
          },
          _ => new
          {
              Event = "view",
              Value = room.Game.View,
              State = room.Game.state
          }
      };

      await BroadcastAsync(room, payload);
    }
    
    private static async Task BroadcastAsync(Room room, object payload)
    {
      string jsonString = JsonConvert.SerializeObject(payload, Formatting.Indented);
      var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);

      foreach (var cl in room.Clients.ToArray())
      {
        var client = cl.Value;
        try
        {
          await client.Socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Error broadcasting board to clients {ex.Message}");
          room.Clients.TryRemove(client.ClientID, out _);
        }
      }
    }

    public static async Task SendBoardToClient(object view, ClientInfo client, Room room)
    {
      // TODO: Actually test function & ensure it is properly sent/recieved by clients on frontend
      room.Game.Players.TryGetValue(client.ClientID, out int player_index);
      BoardData dataToSend = new BoardData { Event = "view", Value = view, State = room.Game.state, Player_Index = player_index}; // Convert board to update object
      string jsonString = JsonConvert.SerializeObject(dataToSend, Formatting.Indented); // Convert update to JSON object
      var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString); // Convert JSON to byte buffer
      try
      {
        await client.Socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error Sending board to client {ex.Message}");
      }
    }

    public class BoardData
    {
      public required string Event { get; set; }
      public required object Value { get; set; }
      public required State State { get; set; }
      public int Player_Index { get; set; }
    }

    public static async Task JoinOrCreateRoomAsync(Guid roomID, string gameKey, ClientInfo client)
    {
      // Inputs: Id of the room, the string for the GameFactory to return a new GameHandler, and finally the clientInfo object themselves for use with adding them to the room
      // Outputs: Just does an action of creating/adding them to the room or just adding them to the one that exists
      var room = rooms.GetOrAdd(roomID, new Room { RoomID = roomID, Game = GameFactory.CreateGame(gameKey) });
      room.Clients[client.ClientID] = client;
      
      room.Game.Join(client);

      // Send board to client
      await SendBoardToClient(room.Game.View, client, room);
    }

    public static (bool found, Guid roomId, string gameKey) QuickPlay()
    {
      var snapshot = rooms.Values.ToArray().ToList();
      foreach (var room in snapshot)
      {
        if (!room.QuickPlayLocked && room.IsOpen)
        {
          return (true, room.RoomID, room.Game.GameKey);
        }
      }
      // No Free Rooms
      return (false, Guid.Empty, string.Empty);
    }

    public static void LeaveRoom(ClientInfo client)
    {
      // Description: Used to remove a client from a room, afterwards if the room is empty it will delete the room itself. Basic cleanup function.
      // Inputs: The client themselves
      // Outputs: No return just removes the client from the room
      var room = FindRoomByRoomID(client.RoomID);
      if (room == null) { return; }

      room.Clients.TryRemove(client.ClientID, out _);

      if (room.Clients.IsEmpty)
      {
        rooms.TryRemove(room.RoomID, out _);
      }
    }
  }

  public class Room
  {
    // Description: Class that is meant to represent a singular room.
    // Inputs: This requires a RoomID which is passed from the frontend url a game itself made from the GameFactory in the "JoinOrCreateRoom" method and finally a dictionary of clients
    // Outputs: Singular room that is stored in the rooms dict
    public required Guid RoomID { get; init; }
    public required GameHandler Game { get; init; }
    public object RoomLock { get; } = new();
    public ConcurrentDictionary<Guid, ClientInfo> Clients { get; } = new();
    public bool QuickPlayLocked = false;
    public bool IsOpen => Game.Players.Count < Game.MaxPlayers && Game.state == State.Playing;
  }
}
