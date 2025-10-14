using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

class Program
{

	static async Task Echo(WebSocket webSocket)
	{
	  var buffer = new byte[1024 * 4];
	  var receiveResult = await webSocket.ReceiveAsync(
		new ArraySegment<byte>(buffer), CancellationToken.None);

	  while (!receiveResult.CloseStatus.HasValue)
	  {
		await webSocket.SendAsync(
		  new ArraySegment<byte>(buffer, 0, receiveResult.Count),
		  receiveResult.MessageType,
		  receiveResult.EndOfMessage,
		  CancellationToken.None);

		receiveResult = await webSocket.ReceiveAsync(
		  new ArraySegment<byte>(buffer), CancellationToken.None);
	  }

	  await webSocket.CloseAsync(
		receiveResult.CloseStatus.Value,
		receiveResult.CloseStatusDescription,
		CancellationToken.None);
	}

	static void Main(string[] args){
		var builder = WebApplication.CreateBuilder(args);
		var app = builder.Build();

		// Configuring the app to serve files int static/...
		app.UseDefaultFiles(new DefaultFilesOptions
		{
			DefaultFileNames = new List<string> { "html/index.html" }
		});
		app.UseStaticFiles();
		app.MapStaticAssets();

		// Configuring web sockerts to start
		app.UseWebSockets();
		app.Use(async (context, next) =>
		{
		  if (context.Request.Path == "/ws")
		  {
			if (context.WebSockets.IsWebSocketRequest)
			{
			  using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
			  Console.WriteLine("Websocket connected");
			  await Echo(webSocket);
			}
			else
			{
			  context.Response.StatusCode = StatusCodes.Status400BadRequest;
			}
		  }
		  else
		  {
			await next(context);
		  }
		});
		app.Run();
	}
}
