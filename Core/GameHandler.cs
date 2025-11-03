using System.Collections;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualBasic;
using SocketHandler.Core;

namespace Game.Core
{
  public enum State {Win, Draw, Playing}
  public abstract class GameHandler
  {
    public abstract IList board { get; }
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

  // TODO: Implement play and WinDetection to both update the board and then check if there is a win or not and update the underlining variable.
  // Notes: Might want to implement a "Winner" public variable so the frontend can read who actually won the game and display that properly. Could
    // also keep a "Next turn" var to always check against the next symbol expected so if O gets played but we want an X we skip this turn
    // Also since state is passed as a string we will most likely convert this from string -> JSON object then parse to get key value pairs
  public class TicTacToe : GameHandler
  {
    // Description: A class that inherits from GameHandler that defines the playing of the tictactoe game
    // Inputs: Other files will call to the Play method passing it a "state" string, this will be passed from the frontend in JSON format but the exact layout is still to be determined.
      // Ex: {event: "click", symbol: "X"} could be a state we can define/handle here
    // Outputs: This class mainly does actions on it's underlyining board object, By doing this it allows for other classes to get that board state whenever and refer to it's current state
      // The Play class however will return a bool that will be based on whether or not a play has been made (Ex: If user A's turn and B clicks return false and we don't have to
      // worry about client side turn checking making the server the one true point of truth)
    public enum Cell { Empty, X, O }
    public override IList board { get; } = new List<Cell>(Enumerable.Repeat(Cell.Empty, 9)); // The board state initalized using list comprehension to a list of 9 empty cells
    private State _state = State.Playing; // Set the inital state to "playing" to signify a match has started
    public override State state => _state; // Allow other outside classes to get the state at any time while not updating it as updating will only happen within this class

    public override bool Play(string state)
    {
      // Description: Method that "plays" a state onto the board, makes sure the move is valid first i.e: in bounds. valid move and more then checks to see if the game is won/draw and updates underlyining state
      // Inputs: Will recieve a state JSON string (JSON strings look like dictionaries in python with key value pairs) from the frontend
      // Outputs: Will update the underlyining board and return a bool saying if a move was done this turn or not
      // TODO: Base Case checking (Is playing + In bounds + correct turn + other checks)

      // TODO: Play the move

      WinDetection(); // Used to update State
      return true; // If nothing stopped it/the play was made we return true since we did a move
    }

    private void WinDetection()
    {
      // Set current state of _state to whatever it needs to be after that move was played. I.E: "Draw" (States in an enum at top if wanting to look at valid states)
      // Inputs: Nothing just looks at the board state
      // Outputs: Nothing just updates the underlyining _state variable
      // TODO: WinDection logic
    }

  }
}
