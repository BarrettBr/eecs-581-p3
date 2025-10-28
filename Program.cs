using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using SocketHandler.Core;

class Program
{
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

		// Configuring web sockets to start
		app.UseWebSockets();
		app.Use(async (context, next) =>
		{
		  if (context.Request.Path == "/ws")
		  {
        if (context.WebSockets.IsWebSocketRequest)
        {
          await WebSocketHandler.HandleWebSocket(context);
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
