namespace WebGames.Tests;
using Game.Core;
using Xunit;

// This is made using xUnit for more information please check the "Testing" section in the README.md.
// Also the "Resources" section has links to easy to digest articles and documentation pages
// Created a bunch of "starter tests" can create more later if needing to test turn variables and what not to ensure they are properly updated upon a turn or no turn
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
  public void FactoryCaseTest()
  {
    var game = GameFactory.CreateGame("TiCtAcToe");
    Assert.IsType<TicTacToe>(game);
  }
  [Fact]
  public void FactoryUnknownTest()
  {
    var game = GameFactory.CreateGame("");
    Assert.IsType<TicTacToe>(game);
  }
  [Fact]
  public void InitialStateTest()
  {
    // Validate that the initial state is set properly
  }
  [Fact]
  public void JoinTest()
  {
    // Console.WriteLine("Reached JoinTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
  }
  [Fact]
  public void JoinOrderTest()
  {
    // Player 1 joins, Player 2 joins make sure both have correct index in the player list
  }
  [Fact]
  public void RejoinTest()
  {
    // If a client leaves and rejoins validate proper handling of them and the program doesn't crash
  }
  [Fact]
  public void PlayTest()
  {
    // Console.WriteLine("Reached PlayTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
    // Given good data does it play the move and the board/view is correctly updated
  }
  [Fact]
  public void NullStatePlayTest()
  {
    // Given null/empty state data does it play the move and the board/view correct
  }
  [Fact]
  public void MalformedPlayRequestTest()
  {
    // Given that a request comes in with no row or no column does it refuse this properly even if the requester is a correct player
  }
  [Fact]
  public void StateTypeTest()
  {
    // Test that a state passed in with correct data but wrong types is properly handled
  }
  [Fact]
  public void OutOfBoundsPlayTest()
  {
    // Console.WriteLine("Reached OutOfBoundsPlayTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
  }
  [Fact]
  public void OccupiedTest()
  {
    // Test to check that if playing into a cell that already has a symbol it is properly rejected
  }
  [Fact]
  public void WrongTurnPlayTest()
  {
    // Console.WriteLine("Reached WrongTurnPlayTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
  }
  [Fact]
  public void SpectatorPlayTest()
  {
    // Console.WriteLine("Reached SpectatorPlayTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
  }
  [Fact]
  public void PlayAfterEndTest()
  {
    // Move was made after the game is over i.e: State = Win or Draw
  }
  [Fact]
  public void WinDectionTest()
  {
    // Test that given a game with a winning state it properly updates the state variable
  }
  [Fact]
  public void DrawDectionTest()
  {
    // Test that given a game with a draw state it properly updates the state variable
  }
  [Fact]
  public void PlayingDectionTest()
  {
    // Test that given a game with a playing state it properly updates the state variable
  }
  [Fact]
  public void ColumnWinTest()
  {
    // Test that given a game with a column winning state it properly updates the state variable
  }
  [Fact]
  public void RowWinTest()
  {
    // Test that given a game with a row winning state it properly updates the state variable
  }
  [Fact]
  public void DiagonalWinTest()
  {
    // Test that given a game with a diagonal winning state it properly updates the state variable
  }
  [Fact]
  public void PlayerIDTest()
  {
    // If a Player joins make sure that the client id stored in the dictionary matches with the client id made
  }
}
