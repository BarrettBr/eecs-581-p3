using System.Collections.Concurrent;
using Game.Core;

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
    public static Room? FindRoom(int clientID)
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

    public static void HandleState(int clientId, object state)
    {
      var room = FindRoom(clientId);
      if (room == null)
      {
        Console.WriteLine($"Room Not found for client {clientId}");
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
    public List<int> clientIDs = new(); // Might have to change to match actual client IDs in WebSocketHandler
  }
}
