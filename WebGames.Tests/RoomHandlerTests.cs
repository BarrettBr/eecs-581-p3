namespace WebGames.Tests;

using System.Net.WebSockets;
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
  }
  [Fact]
  public void LeaveRoomTest()
  {
    // Test to see if leaving a room removes 1 client from it
  }
  [Fact]
  public void LeaveRoomDeleteRoomTest()
  {
    // If last leaving a room does it delete it
  }
  [Fact]
  public void LeaveRoomNoClientTest()
  {
    // If calling leave room with a client that doesn't exist dont break the code
  }
  [Fact]
  public void HandleStateTest()
  {
    // Create room -> Add 2 clients -> play a good state
  }
  [Fact]
  public void HandleStateMalformedTest()
  {
    // Create room -> Add 2 clients -> play a malformed state
    // Malformed can mean Null, bad form i.e: state = "hello" where the backend gamehandler doesn't know how to handle this
  }
  [Fact]
  public void HandleStateClientNotInRoomTest()
  {
    // In the case of a well-formed client but they don't belong in a room does it crash
  }
  [Fact]
  public void CreateDuplicateRoomTest()
  {
    // If creating a room that already exists does it handle this
  }
  [Fact]
  public void CreateSameRoomTest()
  {
    // If creating same room concurrently does it handle this
  }
}
