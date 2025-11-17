using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using SocketHandler.Core;

/*
Prologue

Authors: Barrett Brown, Adam Berry, Alex Phibbs, Minh Vu, Jonathan Gott
Creation Date: 11/08/2025

Description:
- Boots the minimal ASP.NET Core app, serves static frontend files, and exposes a WebSocket endpoint.
- Requests to `/ws` are upgraded to a WebSocket request and handed off to the WebSocket handler.
- All other requests are served from the wwwroot directory.

Functions:
- Main(string[] args): Configures and starts the web app
  - Sets default file, static file hosting, and WebSocket middleware
  - Routes `/ws` requests to WebSocketHandler.HandleWebSocket

Inputs:
- Command-line args to configure the web host. In our case we don't use anything special
- HTTP requests for static assets
- Websocket requests to '/ws'

Outputs:
- Serves static files (HTTP)
- Upgrades `/ws` requests to WebSocket when valid; otherwise returns HTTP 400 (This is done in the WebSocketHandler but passes through here)
*/


class Program
{
  static void Main(string[] args)
  {
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

    // Leaderboard endpoint only have one so no need for a controller here
    app.MapGet("/api/leaderboard/{gameKey}", async (string gameKey) => {
      var dbHandler = DatabaseHandler.Instance;
      var wins = await dbHandler.GetWins(gameKey);
      return Results.Ok(wins);
    });

    app.Run();
  }
}
