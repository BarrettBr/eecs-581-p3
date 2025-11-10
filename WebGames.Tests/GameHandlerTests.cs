namespace WebGames.Tests;
using Game.Core;
using SocketHandler.Core;
using System.Net.WebSockets;
using System.Text.Json;
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
    var game = new TicTacToe();
    Assert.Equal(State.Playing, game.state);
    for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            Assert.Equal(TicTacToe.Cell.Empty, game.Board[r, c]);
  }
  [Fact]
  public void JoinTest()
  {
    // Console.WriteLine("Reached JoinTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
    var game   = new TicTacToe();
    var client = NewClient();

    game.Join(client);

    Assert.True(game.Players.ContainsKey(client.ClientID));
    Assert.Equal(0, game.Players[client.ClientID]);
  }
  [Fact]
  public void JoinOrderTest()
  {
    // Player 1 joins, Player 2 joins make sure both have correct index in the player list
    var game    = new TicTacToe();
    var client1 = NewClient();
    var client2 = NewClient();

    game.Join(client1);
    game.Join(client2);

    Assert.Equal(0, game.Players[client1.ClientID]);
    Assert.Equal(1, game.Players[client2.ClientID]);
  }
  [Fact]
  public void RejoinTest()
  {
    // If a client leaves and rejoins validate proper handling of them and the program doesn't crash
    var game   = new TicTacToe();
    var client = NewClient();

    game.Join(client);
    var idx = game.Players[client.ClientID];
    game.Join(client);

    Assert.Single(game.Players);
    Assert.Equal(idx, game.Players[client.ClientID]);
  }
  [Fact]
  public void PlayTest()
  {
    // Console.WriteLine("Reached PlayTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
    // Given good data does it play the move and the board/view is correctly updated
    var game   = new TicTacToe();
    var client = NewClient();
    game.Join(client);

    const string state = "{\"Row\":0,\"Col\":0}";
    Assert.True(game.Play(state, client));
    Assert.Equal(TicTacToe.Cell.X, game.Board[0, 0]);
    Assert.Equal(State.Playing, game.state);
  }
  [Fact]
  public void NullStatePlayTest()
  {
    // Given null/empty state data does it play the move and the board/view correct
    var game   = new TicTacToe();
    var client = NewClient();
    game.Join(client);

    var ex = Assert.Throws<ArgumentNullException>(() => game.Play(null!, client));
    Assert.Equal("json", ex.ParamName);
  }
  [Fact]
  public void MalformedPlayRequestTest()
  {
    // Given that a request comes in with no row or no column does it refuse this properly even if the requester is a correct player
    var game   = new TicTacToe();
    var client = NewClient();
    game.Join(client);

    const string state = "{}";
    Assert.Throws<KeyNotFoundException>(() => game.Play(state, client));
  }
  [Fact]
  public void StateTypeTest()
  {
    // Test that a state passed in with correct data but wrong types is properly handled
    var game   = new TicTacToe();
    var client = NewClient();
    game.Join(client);

    const string state = "{\"Row\":\"a\",\"Col\":0}";
    var ex = Assert.Throws<InvalidOperationException>(() => game.Play(state, client));
    Assert.Contains("Number", ex.Message);
  }
  [Fact]
  public void OutOfBoundsPlayTest()
  {
    // Console.WriteLine("Reached OutOfBoundsPlayTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
    var game   = new TicTacToe();
    var client = NewClient();
    game.Join(client);

    const string state = "{\"Row\":3,\"Col\":0}";
    Assert.False(game.Play(state, client));
    Assert.Equal(State.Playing, game.state);
  }
  [Fact]
  public void OccupiedTest()
  {
    // Test to check that if playing into a cell that already has a symbol it is properly rejected
    var game   = new TicTacToe();
    var client = NewClient();
    game.Join(client);

    const string first = "{\"Row\":0,\"Col\":0}";
    game.Play(first, client);
    Assert.False(game.Play(first, client));
  }
  [Fact]
  public void WrongTurnPlayTest()
  {
    // Console.WriteLine("Reached WrongTurnPlayTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
    var game    = new TicTacToe();
    var client1 = NewClient();
    var client2 = NewClient();
    game.Join(client1);
    game.Join(client2);

    game.Play("{\"Row\":0,\"Col\":0}", client1);
    Assert.False(game.Play("{\"Row\":0,\"Col\":1}", client1));
  }
  [Fact]
  public void SpectatorPlayTest()
  {
    // Console.WriteLine("Reached SpectatorPlayTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
    var game      = new TicTacToe();
    var player1   = NewClient();
    var player2   = NewClient();
    var spectator = NewClient();

    game.Join(player1);
    game.Join(player2);
    game.Join(spectator);

    Assert.False(game.Play("{\"Row\":0,\"Col\":0}", spectator));
  }
  [Fact]
  public void PlayAfterEndTest()
  {
    // Move was made after the game is over i.e: State = Win or Draw
    var game    = new TicTacToe();
    var client1 = NewClient();
    var client2 = NewClient();
    game.Join(client1);
    game.Join(client2);

    game.Play("{\"Row\":0,\"Col\":0}", client1);
    game.Play("{\"Row\":1,\"Col\":0}", client2);
    game.Play("{\"Row\":0,\"Col\":1}", client1);
    game.Play("{\"Row\":1,\"Col\":1}", client2);
    game.Play("{\"Row\":0,\"Col\":2}", client1);

    Assert.Equal(State.Win, game.state);
    Assert.False(game.Play("{\"Row\":2,\"Col\":0}", client2));
  }
  [Fact]
  public void WinDectionTest()
  {
    // Test that given a game with a winning state it properly updates the state variable
    PlayToWinAndAssert(State.Win);
  }
  [Fact]
  public void DrawDectionTest()
  {
    // Test that given a game with a draw state it properly updates the state variable
    PlayToDrawAndAssert(State.Draw);
  }
  [Fact]
  public void PlayingDectionTest()
  {
    // Test that given a game with a playing state it properly updates the state variable
    Assert.Equal(State.Playing, new TicTacToe().state);
  }
  [Fact]
  public void ColumnWinTest()
  {
    // Test that given a game with a column winning state it properly updates the state variable
    PlayColumnWinAndAssert(State.Win);
  }
  [Fact]
  public void RowWinTest()
  {
    // Test that given a game with a row winning state it properly updates the state variable
    PlayRowWinAndAssert(State.Win);
  }
  [Fact]
  public void DiagonalWinTest()
  {
    // Test that given a game with a diagonal winning state it properly updates the state variable
    PlayDiagonalWinAndAssert(State.Win);
  }
  [Fact]
  public void PlayerIDTest()
  {
    // If a Player joins make sure that the client id stored in the dictionary matches with the client id made
    var game   = new TicTacToe();
    var client = NewClient();

    game.Join(client);

    Assert.Contains(client.ClientID, game.Players.Keys);
  }

  // Helper methods
  private static ClientInfo NewClient() => new()
  {
      ClientID = Guid.NewGuid(),
      RoomID   = Guid.NewGuid(),
      Socket   = new ClientWebSocket()
  };

  private static void PlayToWinAndAssert(State expected)
  {
      var (game, c1, c2) = SetupTwoPlayers();
      game.Play("{\"Row\":0,\"Col\":0}", c1);
      game.Play("{\"Row\":1,\"Col\":0}", c2);
      game.Play("{\"Row\":0,\"Col\":1}", c1);
      game.Play("{\"Row\":1,\"Col\":1}", c2);
      game.Play("{\"Row\":0,\"Col\":2}", c1);
      Assert.Equal(expected, game.state);
  }

  private static void PlayColumnWinAndAssert(State expected)
  {
      var (game, c1, c2) = SetupTwoPlayers();
      game.Play("{\"Row\":0,\"Col\":0}", c1);
      game.Play("{\"Row\":0,\"Col\":1}", c2);
      game.Play("{\"Row\":1,\"Col\":0}", c1);
      game.Play("{\"Row\":1,\"Col\":1}", c2);
      game.Play("{\"Row\":2,\"Col\":0}", c1);
      Assert.Equal(expected, game.state);
  }

  private static void PlayRowWinAndAssert(State expected)
  {
      var (game, c1, c2) = SetupTwoPlayers();
      game.Play("{\"Row\":0,\"Col\":0}", c1);
      game.Play("{\"Row\":1,\"Col\":0}", c2);
      game.Play("{\"Row\":0,\"Col\":1}", c1);
      game.Play("{\"Row\":1,\"Col\":1}", c2);
      game.Play("{\"Row\":0,\"Col\":2}", c1);
      Assert.Equal(expected, game.state);
  }

  private static void PlayDiagonalWinAndAssert(State expected)
  {
      var (game, c1, c2) = SetupTwoPlayers();
      game.Play("{\"Row\":0,\"Col\":0}", c1);
      game.Play("{\"Row\":0,\"Col\":1}", c2);
      game.Play("{\"Row\":1,\"Col\":1}", c1);
      game.Play("{\"Row\":0,\"Col\":2}", c2);
      game.Play("{\"Row\":2,\"Col\":2}", c1);
      Assert.Equal(expected, game.state);
  }

  private static void PlayToDrawAndAssert(State expected)
  {
      var (game, c1, c2) = SetupTwoPlayers();
      game.Play("{\"Row\":0,\"Col\":0}", c1); game.Play("{\"Row\":0,\"Col\":1}", c2);
      game.Play("{\"Row\":0,\"Col\":2}", c1); game.Play("{\"Row\":1,\"Col\":1}", c2);
      game.Play("{\"Row\":1,\"Col\":0}", c1); game.Play("{\"Row\":1,\"Col\":2}", c2);
      game.Play("{\"Row\":2,\"Col\":1}", c1); game.Play("{\"Row\":2,\"Col\":0}", c2);
      game.Play("{\"Row\":2,\"Col\":2}", c1);
      Assert.Equal(expected, game.state);
  }

  private static (TicTacToe game, ClientInfo p1, ClientInfo p2) SetupTwoPlayers()
  {
      var g  = new TicTacToe();
      var p1 = NewClient(); g.Join(p1);
      var p2 = NewClient(); g.Join(p2);
      return (g, p1, p2);
  }
}