namespace WebGames.Tests;

using Game.Core;
using SocketHandler.Core;

using Xunit;

// This is made using xUnit for more information please check the "Testing" section in the README.md.
// Also the "Resources" section has links to easy to digest articles and documentation pages
public class GameHandlerTester
{
  [Fact]
  public void FactoryTest()
  {
    // Console.WriteLine("Reached FactoryTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
    var game = GameFactory.CreateGame("tictactoe");
    Assert.IsType<TicTacToe>(game);
  }
  [Fact]
  public void PlayTest()
  {
    // Console.WriteLine("Reached PlayTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
  }
}
