using System.ComponentModel;

namespace SocketHandler.Core
{
  public sealed class DatabaseHandler
  {

    // Below bit for instance is used to make this a singleton class since database should be singular
    private static DatabaseHandler? instance = null;
    // Lock
    private static readonly object padlock = new(); // Lock used for making the DatabaseHandler instance work properly
    DatabaseHandler() { }

    public static DatabaseHandler Instance
    {
      get
      {
        lock (padlock)
        {
          instance ??= new DatabaseHandler();
          return instance;
        }
      }
    }
    public static async Task UpdateWin(ClientInfo client)
    {
      // Lookup client in database -> increment win number -> make sure this displays somewhere on site
      // Maybe update clientInfo to include a "display name" if needed otherwise include a pseudorandom name for them
      // Since every game has its own short-lived ws socket if user 1 wins ttt goes to rps those count as 2 wins so handle this
    }
  }
}
