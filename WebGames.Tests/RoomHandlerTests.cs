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
}
