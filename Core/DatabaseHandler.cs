using System.ComponentModel;
using System.Data.SQLite;
using Game.Core;

/*
Prologue

Authors: Barrett Brown
Creation Date: 11/20/2025

Description:
- Implements a singleton-style database manager ('DatabaseHandler') responsible for all interactions with the SQLite backend.
- Designed to support multiple games by mapping each gameKey to its corresponding player statistics table.
- Provides methods to update and retrieve win counts for players across different game types.
- Ensures thread-safe access to the SQLite database through a locked singleton instance, preventing multiple conflicting connections.
- Used by the Room/Game system to persist player performance and support future leaderboard features.

Functions / Classes:
- sealed class DatabaseHandler:
    - Instance: Provides global, thread-safe access to the single DatabaseHandler instance.
    - tableNameFinder(gameKey: string): Maps an internal game identifier to its corresponding database table.
    - UpdateWin(room: Room, alias: string): Increments a player's win count or inserts a new record if the player does not yet exist.
    - GetWinsForUser(gameKey: string, alias: string): Returns the total wins for the specified player in the specified game.
    - GetWins(gameKey: string): Returns all aliases and win totals for the given game in a dictionary.

Inputs:
- gameKey strings identifying the game mode (e.g., "tictactoe").
- alias strings chosen by players when joining a room.
- Room objects used to determine the correct table based on the game type.

Outputs:
- Updated SQLite tables storing user win statistics.
- Integer win counts for individual players.
- Dictionaries mapping player aliases to their win totals.

*/


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

    public string tableNameFinder(string gameKey)
    {
      return gameKey.ToLowerInvariant() switch
      {
        "tictactoe" => "TicTacToePlayers",
        _ => throw new NotImplementedException("Game type not found for database table lookup"),
      };
    }

    public async Task UpdateWin(Room room, string alias)
    {
      // Lookup client in database -> increment win number -> make sure this displays somewhere on site
      // Sent room to identify game type since different games may have different database tables, also sent alias since
      // the player sets an alias when they start the game that is sent along with the win state to track them
      string tableName = tableNameFinder(room.Game.GameKey);
      using var connection = new SQLiteConnection("Data Source=game_database.db;Version=3;");
      connection.Open();

      // Setup basic command to update wins if player already exists
      using var command = new SQLiteCommand(connection);
      command.CommandText = $"UPDATE {tableName} SET Wins = Wins + 1 WHERE Alias = @alias";
      command.Parameters.AddWithValue("@alias", alias); // Use alias to identify player
      
      // Check to see if update happened
      int rowsAffected = await command.ExecuteNonQueryAsync();
      if (rowsAffected == 0)
      {
        // No rows affected means alias not found, so insert new record
        command.CommandText = $"INSERT INTO {tableName} (Alias, Wins) VALUES (@alias, 1)";
        await command.ExecuteNonQueryAsync();
      }
      connection.Close();
    }

    public async Task<int> GetWinsForUser(string gameKey, string alias)
    {
      // Get number of wins for a player based on their alias and game type
      string tableName = tableNameFinder(gameKey);

      using var connection = new SQLiteConnection("Data Source=game_database.db;Version=3;");
      connection.Open();

      using var command = new SQLiteCommand(connection);
      command.CommandText = $"SELECT Wins FROM {tableName} WHERE Alias = @alias";
      command.Parameters.AddWithValue("@alias", alias);

      var result = await command.ExecuteScalarAsync();
      connection.Close();

      return result != null ? Convert.ToInt32(result) : 0; // if results is not null return wins else 0
    }

    public async Task<Dictionary<string, int>> GetWins(string gameKey)
    {
      // Get number of wins and player alias for a game type
      string tableName = tableNameFinder(gameKey);

      using var connection = new SQLiteConnection("Data Source=game_database.db;Version=3;");
      connection.Open();

      using var command = new SQLiteCommand(connection);
      command.CommandText = $"SELECT Alias, Wins FROM {tableName}";

      var result = await command.ExecuteReaderAsync();

      // Return a dictionary of alias and wins
      var winsDict = new Dictionary<string, int>();
      while (await result.ReadAsync())
      {
        string alias = result.GetString(0);
        int wins = result.GetInt32(1);
        winsDict[alias] = wins; 
      }
      connection.Close();
      return winsDict;
    }
  }
}
