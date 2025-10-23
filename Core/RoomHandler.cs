namespace SocketHandler.Core
{
  public sealed class RoomHandler
  {
    // Room instance
    private static RoomHandler? instance = null;

    // Lock
    private static readonly object padlock = new object();

    private static List<Room> rooms = new();

    // Empty Class Constructor
    RoomHandler()
    {
    }

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
      foreach (var room in rooms)
      {
        if (room.roomID.Contains(clientID))
        {
          return room;
        }
      }
      return null;
    }
  }

  // Filler class for a room for now
  public class Room
  {
    public List<int> roomID = [];
  }

  public class ClientInfo
  {
    
  }
}
