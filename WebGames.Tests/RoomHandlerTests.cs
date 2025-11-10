namespace WebGames.Tests;

using System.Net.WebSockets;
using System.Threading.Tasks;
using Game.Core;
using SocketHandler.Core;
using Xunit;

public class RoomHandlerTester
{
  [Fact]
  public void CreateRoomTest()
  {
    // Console.WriteLine("Reached CreateRoomTest"); // Any Console.WriteLines() will be printed mid-test so useful for debugging
    // Create a Client/RoomId to use later
    var roomId = Guid.NewGuid();
    var clientId = Guid.NewGuid();
    var client = new ClientInfo
    {
      ClientID = clientId,
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    // Create a room then fetch it
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client);
    var room = RoomHandler.FindRoomByRoomID(roomId);

    // Test that the room actually exists
    Assert.NotNull(room);
  }
  [Fact]
  public void FindRoomByClientTest()
  {
    // Create a client/room like CreateRoomTest and then find the room by the clients id
    var roomId = Guid.NewGuid();
    var client1 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    var client2 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client1);
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client2);

    var room = RoomHandler.FindRoomByClientID(client2.ClientID);

    Assert.NotNull(room);
    Assert.Equal(roomId, room.RoomID); 
  }
  [Fact]
  public void LeaveRoomTest()
  {
    // Test to see if leaving a room removes 1 client from it
    var roomId = Guid.NewGuid();
    var client1 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    var client2 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client1);
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client2);
    var room = RoomHandler.FindRoomByRoomID(roomId);

    // TODO: Remove client from room as of now there are 2 clients in this room
    RoomHandler.LeaveRoom(client2);

    Assert.NotNull(room);
    Assert.Single(room.Clients); 
  }
  [Fact]
  public void LeaveRoomDeleteRoomTest()
  {
    // If last leaving a room does it delete it
    var roomId = Guid.NewGuid();
    var client = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client);

    RoomHandler.LeaveRoom(client);
    var room = RoomHandler.FindRoomByRoomID(roomId);

    Assert.Null(room); 
  }
  [Fact]
  public void LeaveRoomNoClientTest()
  {
    // If calling leave room with a client that doesn't exist dont break the code
    var roomId = Guid.NewGuid();
    var client = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    var nonClient = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    // TODO: Finish this, currently code creates an unused client makes another
      // leave a room that isnt in one then checks if a room exists and returns if the room exists but the room was never made
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client);
    RoomHandler.LeaveRoom(nonClient);
    var room = RoomHandler.FindRoomByRoomID(roomId);

    Assert.NotNull(room);
    Assert.Single(room.Clients); 
  }
  [Fact]
  public async Task HandleStateTest()
  {
    // Create room -> Add 2 clients -> play a good state
    var roomId = Guid.NewGuid();
    var client1 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    var client2 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client1);
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client2);

    var room = RoomHandler.FindRoomByRoomID(roomId);
    Assert.NotNull(room); 
    var board_before = ((TicTacToe)room.Game).Board[0, 0]; 

    // TODO: Recommend matching this to the wwwroot/js/ttt.js move event format as that is what should be recognized by the gamehandler
      // Otherwise it might cause an error down the line where 2 areas read it in different
    var state = "{\"Row\":0,\"Col\":0}";

    await RoomHandler.HandleStateAsync(client1, state);

    var board_after = ((TicTacToe)room.Game).Board[0, 0];

    Assert.NotEqual(board_before, board_after); 
  }
  [Fact]
  public async Task HandleStateMalformedTest()
  {
    // Create room -> Add 2 clients -> play a malformed state
    // Malformed can mean Null, bad form i.e: state = "hello" where the backend gamehandler doesn't know how to handle this
    var roomId = Guid.NewGuid();
    var client = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client);

    var room = RoomHandler.FindRoomByRoomID(roomId);
    Assert.NotNull(room); 
    var board_before = ((TicTacToe)room.Game).Board[0, 0];

    var badstate1 = "not_json";
    var badstate2 = "{\"wrong\":true}";
    var badstate3 = "{\"Row\":100,\"Col\":0}";

    await RoomHandler.HandleStateAsync(client, badstate1);
    await RoomHandler.HandleStateAsync(client, badstate2);
    await RoomHandler.HandleStateAsync(client, badstate3);

    var board_after = ((TicTacToe)room.Game).Board[0, 0];

    Assert.Equal(board_before, board_after); 
  }
  [Fact]
  public async Task HandleStateClientNotInRoomTest()
  {
    // In the case of a well-formed client but they don't belong in a room does it crash
    var roomId = Guid.NewGuid();
    var client = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client);

    var nonClient = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    var room = RoomHandler.FindRoomByRoomID(roomId);
    Assert.NotNull(room); 
    var board_before = ((TicTacToe)room.Game).Board[1, 1];

    // TODO: Recommend matching this to the wwwroot/js/ttt.js move event format as that is what should be recognized by the gamehandler
      // Otherwise it might cause an error down the line where 2 areas read it in different
    var state = "{\"Row\":1,\"Col\":1}";

    await RoomHandler.HandleStateAsync(nonClient, state);

    var board_after = ((TicTacToe)room.Game).Board[1, 1];

    Assert.Equal(board_before, board_after); 
  }
  [Fact]
  public void CreateDuplicateRoomTest()
  {
    // If creating a room that already exists does it handle this
    var roomId = Guid.NewGuid();
    var client1 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    var client2 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client1);
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client2);

    var room = RoomHandler.FindRoomByRoomID(roomId);
    Assert.NotNull(room);

    Assert.Equal(roomId, room.RoomID);
    Assert.Equal(2, room.Clients.Count); 
  }
  [Fact]
  public async Task CreateSameRoomTest()
  {
    // If creating same room concurrently does it handle this
    var roomId = Guid.NewGuid();
    var client1 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    var client2 = new ClientInfo
    {
      ClientID = Guid.NewGuid(),
      RoomID = roomId,
      Socket = new ClientWebSocket()
    };

    // TODO: this test is more about the idea of "concurrently creating rooms" this means running each on a seperate thread and sending the request at the same time
    // At the moment this is trying to create/join a room then do it again
    // Look into multithreading in c# to run each on a different thread and look into running at around same time to simulate this
    // Should end the join/create multithread and afterwards check to see if it was handled or if it happened differently/caused errors
    var t1 = Task.Run(() => RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client1));
    var t2 = Task.Run(() => RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client2));
    await Task.WhenAll(t1, t2);

    var room = RoomHandler.FindRoomByRoomID(roomId);
    Assert.NotNull(room);
    Assert.Equal(2, room.Clients.Count);
  }
  [Fact]
  public void QuickPlayTest()
  {
    // Test adding a client to a room and then having another join through quickplay
    var roomId = Guid.NewGuid();
    var p1 = NewClient(roomId);
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", p1);

    var p2 = NewClient(Guid.Empty);
    var (joined, returnedRoomId, gameKey) = RoomHandler.QuickPlay(p2);

    Assert.True(joined);
    Assert.Equal(roomId, returnedRoomId);
    Assert.Equal("tictactoe", gameKey);
  }
  [Fact]
  public void QuickPlayRoomsFullTest()
  {
    // Test adding a client to a room when rooms are full
    var roomId = Guid.NewGuid();
    var p1 = NewClient(roomId);
    var p2 = NewClient(roomId);
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", p1);
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", p2);

    var p3 = NewClient(Guid.Empty);
    var (joined, _, _) = RoomHandler.QuickPlay(p3);
    Assert.False(joined);
  }
  [Fact]
  public void QuickPlayMalformedClientTest()
  {
    // Test adding a malformed client to a room via quickplay
    Assert.Throws<NullReferenceException>(() => RoomHandler.QuickPlay(null!));
  }

  // Helper methods
  private static ClientInfo NewClient(Guid roomId) => new()
  {
      ClientID = Guid.NewGuid(),
      RoomID   = roomId,
      Socket   = new ClientWebSocket()
  };

  private static (Room room, ClientInfo c1, ClientInfo c2) SetupTwoPlayerRoom()
  {
      var roomId = Guid.NewGuid();
      var c1 = NewClient(roomId);
      var c2 = NewClient(roomId);

      RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", c1);
      RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", c2);

      var room = RoomHandler.FindRoomByRoomID(roomId)!;
      return (room, c1, c2);
  }
}
