using Microsoft.Net.Http.Headers;
using Microsoft.VisualBasic;

namespace Game.Core
{
  public enum Cell { Empty, X, O }
  public enum State {Win, Lose, Draw, Playing}
  public abstract class GameHandler
  {
    public abstract List<Cell> board { get; }
    public abstract bool Play(string state);
    public abstract State state { get; }
  }

  // TODO: Implement play and WinDetection to both update the board and then check if there is a win or not and update the underlining variable.
  // The st
  public class TicTacToe : GameHandler
  {
    // Declare an override for the required board variable in the abstract class
    public override List<Cell> board { get; } = new(Enumerable.Repeat(Cell.Empty, 9));
    private State _state = State.Playing;
    public override State state => _state;

    // Output: Bool based on whether a move was made or not
    public override bool Play(string state)
    {
      // TODO: Base Case checking

      WinDetection(); // Used to update State
      return true; // If nothing stopped it/the play was made we return true since we did a move
    }

    // Set current state of _state to whatever it needs to be use state as a getter to refer to value outside of class
    private void WinDetection()
    {
      
    }

  }
}
