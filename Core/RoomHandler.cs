using System.Collections.Concurrent;
using Game.Core;
using Microsoft.AspNetCore.StaticAssets;

namespace SocketHandler.Core
{
  public sealed class RoomHandler
  {
    // Room instance
    private static RoomHandler? instance = null;
    // Lock
    private static readonly object padlock = new object();
    private static readonly ConcurrentDictionary<Guid, Room> rooms = new();

    // Empty Class Constructor
    RoomHandler(){}

    public static RoomHandler Instance
    {
      get
      {
        lock (padlock)
        {
          if (instance == null)
          {
            instance = new RoomHandler();
          }
          return instance;
        }
      }
    }

    // Method used as a helper to get the room that a client is connected to.
    public static Room? FindRoomByClientID(Guid clientID)
    {
      // Loop through internal area of connections finding which room these go to and 
      foreach (var room in rooms.Values)
      {
        if (room.clientIDs.Contains(clientID))
        {
          return room;
        }
      }
      return null;
    }

    public static Room? FindRoomByRoomID(Guid roomID)
    {
      return rooms.GetValueOrDefault(roomID);
    }
    public static void HandleState(ClientInfo client, string state)
    {
      var room = FindRoomByRoomID(client.RoomId);
      if (room == null)
      {
        Console.WriteLine($"Room Not found for client {client.ClientId}");
        return;
      }
      try
      {
        if (room.game.Play(state))
        {
          // Send out board
          var board = room.game.board;
        }
      }
      catch (Exception e)
      {
        Console.WriteLine($"Error handling state {e.Message}");
      }
    }

    public static Guid CreateRoom(GameHandler game)
    {
      var id = Guid.NewGuid();
      rooms[id] = new Room{game=game, roomID=id};
      return id;
    }

    public static bool JoinRoom()
    {

      return true;
    }
    
    public static void LeaveRoom()
    {
      
    }
  }

  // Filler class for a room for now
  public class Room
  {
    public required Guid? roomID;
    public required GameHandler game;
    public List<Guid> clientIDs = new(); // Might have to change to match actual client IDs in WebSocketHandler
  }
}
