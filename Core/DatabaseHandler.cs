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
    public async Task UpdateWin(Room room, string alias)
    {
      // Lookup client in database -> increment win number -> make sure this displays somewhere on site
      // Sent room to identify game type since different games may have different database tables, also sent alias since
      // the player sets an alias when they start the game that is sent along with the win state to track them
      await Task.CompletedTask; // Placeholder to make this async
    }
  }
}
