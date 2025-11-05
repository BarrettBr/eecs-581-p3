using System.Collections;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualBasic;
using SocketHandler.Core;

namespace Game.Core
{
  public enum State {Win, Draw, Playing}
  public abstract class GameHandler
  {
    public abstract object View { get; }
    public abstract string GameKey { get; }
    public abstract bool Play(string state);
    public abstract State state { get; }
  }

  public static class GameFactory
  {
    public static GameHandler CreateGame(string gameKey)
    {
      return gameKey.ToLowerInvariant() switch
      {
        "tictactoe" => new TicTacToe(),
        _ => new TicTacToe(),// TODO: Set to break + handling; Currently set default to TicTacToe but can change to break + handle for later just set it for testing reasons
      };
    }
  }

  // TODO: Implement Play() and WinDetection().
  //  Play() should update the board and check if the move is valid.
  //  WinDetection() should evaluate the board and update the current State.
  // Notes:
  //  Consider adding a public "Winner" field to the View so the frontend can display who won.
  //  You may also add a "NextTurn" variable to track whose move is expected.
  //  The Play() method will likely parse a JSON string (state) into key/value data.
  public class TicTacToe : GameHandler
  {
    // Description:
    //  Defines the TicTacToe game logic that inherits from GameHandler.
    //
    // Inputs:
    //    The Play() method receives a JSON string from the frontend describing a move.
    //    Example: { "event": "click", "symbol": "X" }.
    //
    // Outputs:
    //    Updates the internal board state and exposes it through the View property.
    //    Returns a bool indicating whether the move was successfully applied.
    //    The server acts as the single source of truth for game state and turn order.

    public enum Cell { Empty, X, O }
    public override string GameKey => "tictactoe"; // Added in case we need to tell the game in roomhandler down the line
    public Cell[,] Board { get; } = new Cell[3,3]; // The board state initalized using list comprehension to a list of 9 empty cells
    private State _state = State.Playing; // Set the inital state to "playing" to signify a match has started
    public override State state => _state; // Allow other outside classes to get the state at any time while not updating it as updating will only happen within this class
    public override object View => Board;
    public override bool Play(string state)
    {
      // Description:
      //  Handles a player's move. Verifies that the move is valid (e.g., within bounds,
      //  correct turn, and not overwriting an existing cell), then applies it to the board.
      //
      // Inputs:
      //    JSON string from the frontend representing the move.
      // Outputs:
      //    Updates the board and returns true if a valid move was made.
      //
      // TODO:
      //   Add base case validation (is game still playing, bounds check, correct turn, etc.)
      //   Apply the move once validated.


      WinDetection(); // Used to update State
      return true; // If nothing stopped it/the play was made we return true since we did a move
    }

    private void WinDetection()
    {
      // Description:
      //  Checks the board for win or draw conditions and updates _state accordingly.
      //
      // Inputs: None — uses the current board state.
      // Outputs: None — updates the internal _state variable.
      //
      // TODO: Implement win/draw detection logic.

    }

  }
}
