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

    RoomHandler.LeaveRoom(nonClient);
    var room = RoomHandler.FindRoomByClientID(roomId);
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

    var state = "{\"row\":0,\"col\":0}";

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
    var badstate3 = "{\"row\":100}";

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

    var state = "{\"row\":1,\"col\":1}";

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
  public void CreateSameRoomTest()
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

    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client1);
    RoomHandler.JoinOrCreateRoom(roomId, "tictactoe", client2);

    var room = RoomHandler.FindRoomByRoomID(roomId);
    Assert.NotNull(room);
    Assert.Equal(2, room.Clients.Count); 
  }
}
