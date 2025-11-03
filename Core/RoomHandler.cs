using System.Collections.Concurrent;
using Game.Core;
using Microsoft.AspNetCore.StaticAssets;
using Microsoft.VisualBasic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Collections;

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
    RoomHandler(){}

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
      foreach(var room in rooms.Values)
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
    public static async Task HandleStateAsync(ClientInfo client, string state)
    {
      // Description: Takes a state from the frontend (ex NOT based on current state: {"click", row, col}) and passed to the GameHandler to handle/deal with
      // Inputs: Takes in the state update from from the frontend and passes to the backend gamehandler to run it on it's current board
      // Outputs: Doesn't return a value, instead it will let the gamehandler play method handle the state afterwards it passes off the new board to BroadcastBoard to send it out to all sockets
      var room = FindRoomByRoomID(client.RoomID);
      if (room == null)
      {
        Console.WriteLine($"Room Not found for client {client.ClientID}");
        return;
      }
      try
      {
        // Added lock to prevent 2 users from playing at the same time
        bool changed;
        lock (room.RoomLock)
        {
          changed = room.Game.Play(state);
        }
        if (changed)
        {
          await BroadcastBoard(room.Game.board, room);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine($"Error handling state {e.Message}");
      }
    }

    // Used to convert enums to data as Text.Json converts it to numbers by default
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
      Converters = { new JsonStringEnumConverter() }
    };

    public static async Task BroadcastBoard(IList board, Room room)
    {
      // TODO: Actually test function & ensure it is properly sent/recieved by clients on frontend
      BoardData dataToSend = new BoardData { Message = "boardUpdate", Value = board }; // Convert board to update object
      string jsonString = JsonSerializer.Serialize(dataToSend, JsonOptions); // Convert update to JSON object
      var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString); // Convert JSON to byte buffer
      // Loop over each client in the room and send them the update
      
      foreach (var cl in room.Clients.ToArray())
      {
        var client = cl.Value;
        try
        {
          await client.Socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        } catch (Exception ex){
          Console.WriteLine($"Error broadcasting board to clients {ex.Message}");
          room.Clients.TryRemove(client.ClientID, out _);
        }
      }
    }

    public static async Task SendBoardToClient(IList board, ClientInfo client)
    {
      // TODO: Actually test function & ensure it is properly sent/recieved by clients on frontend
      BoardData dataToSend = new BoardData { Message = "boardUpdate", Value = board }; // Convert board to update object
      string jsonString = JsonSerializer.Serialize(dataToSend, JsonOptions); // Convert update to JSON object
      var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString); // Convert JSON to byte buffer
      try
      {
        await client.Socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
      } catch (Exception ex){
        Console.WriteLine($"Error Sending board to client {ex.Message}");
      }
    }

    public class BoardData
    {
      public required string Message { get; set; }
      public required IList Value { get; set; }
    }

    public static void JoinOrCreateRoom(Guid roomID, string gameKey, ClientInfo client)
    {
      // Inputs: Id of the room, the string for the GameFactory to return a new GameHandler, and finally the clientInfo object themselves for use with adding them to the room
      // Outputs: Just does an action of creating/adding them to the room or just adding them to the one that exists
      var room = rooms.GetOrAdd(roomID, new Room { RoomID = roomID, Game = GameFactory.CreateGame(gameKey) });
      room.Clients[client.ClientID] = client;

      // Send board to client
      _ = SendBoardToClient(room.Game.board, client);
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
    // TODO: Handle spectators seperatorly from all clients as of right now we just add everyone connected to a clients pool.
      // Maybe do it on order added to dict i.e: first joined player 1 and so on and let the backend GameHandler handle that in cases of 3+ player games?
    // Description: Class that is meant to represent a singular room.
    // Inputs: This requires a RoomID which is passed from the frontend url a game itself made from the GameFactory in the "JoinOrCreateRoom" method and finally a dictionary of clients
    // Outputs: Singular room that is stored in the rooms dict
    public required Guid RoomID { get; init; }
    public required GameHandler Game { get; init; }
    public object RoomLock { get; } = new();
    public ConcurrentDictionary<Guid, ClientInfo> Clients { get; } = new();
  }
}
