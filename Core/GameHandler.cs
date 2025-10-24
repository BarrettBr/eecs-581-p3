using Microsoft.Net.Http.Headers;
using Microsoft.VisualBasic;

namespace Game.Core
{
  public enum Cell {Empty, X, O}
  public abstract class GameHandler
  {
    public abstract List<Cell> board { get; }
    public abstract bool Play(object state);
  }

  public class TicTacToe : GameHandler
  {
    // Declare an override for the required board variable in the abstract class
    public override List<Cell> board { get; } = new(Enumerable.Repeat(Cell.Empty, 9));

    // Output: Bool based on whether a move was made or not
    public override bool Play(object state)
    {


      return true;
    }
    
  }
}
