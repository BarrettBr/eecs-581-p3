using Microsoft.Net.Http.Headers;
using Microsoft.VisualBasic;

namespace Game.Core
{
  public enum Cell { Empty, X, O }
  public enum State {Win, Lose, Draw, Playing}
  public abstract class GameHandler
  {
    public abstract List<Cell> board { get; }
    public abstract bool Play(object state);
    public abstract State state { get; }
  }

  // TODO: Implement play and WinDetection to both update the board and then check if there is a win or not and update the underlining variable.
  // The st
  public class TicTacToe : GameHandler
  {
    // Declare an override for the required board variable in the abstract class
    public override List<Cell> board { get; } = new(Enumerable.Repeat(Cell.Empty, 9));
    public override State state { get; } = State.Playing;

    // Output: Bool based on whether a move was made or not
    public override bool Play(object state)
    {
      // TODO: Base Case checking

      WinDetection(); // Used to update State
      return true; // If nothing stopped it/the play was made we return true since we did a move
    }

    // Set current state to whatever it needs to be
    public void WinDetection()
    {
      
    }

  }
}
