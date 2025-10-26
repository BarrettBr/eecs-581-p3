# EECS 581 Project 3 (Tic-Tac-Toe Full Stack Project)

Super cool tic-tac-toe project

## Resources

- [Websocket Support in ASP.NET Core - Basically a C# websocket getting started page](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-9.0)
- [.Net Dependency Download the .Net SDK v9+](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Pico css documentation](https://picocss.com/docs)
- (OPTIONAL) Used for testing echo reponse if you want to NOT NEEDED, and many other ways to test this. Just a "If you want to _really_ be sure [NPM Node.Js Download](https://nodejs.org/en/download)

## Structure

- `wwwroot/` is where all static files are located. This includes html, css, and js files. Something to be aware of is that every file in `wwwroot` can be served to a user so it is best practice to keep things that don't need to be there out of it.
- `Core/` stores all of the C# files except for `Program.cs` which serves as the entry point for the app
- `Documents/` holds all of our artifacts such as our requirements spreadsheet etc for assignments

## Notes

- Note at top of "Websocket Support in ASP.NET Core" page said that chrome/edge have http/2 websockets enabled by default but if you are in firefox you might have to enable it. Idk if we will/are going to use that specifically but just in case some error happens and you dont know why it is a good check

## Testing

- (Dependency Test/making sure you are good to go) Connect to WS server/echo back information
  - Starting Server: Run `dotnet run` in the folder with "Program.cs"
  - To test using the front end html/js files open the link `http://localhost:5238` (port could differ just look in the terminal to get what it says) in the browser of your choice

## File Descriptions

- Program.cs: Entry point of the program, it is where the basic application is compiled together, static files told to be served out, and the websocket listener starts
- WebSocketHandler.cs: This file will be what handles the websocket requests recieved. This includes getting the client a GUID, creating a ClientInfo profile for them and passing them off to RoomHandler to work with their state. This file is mainly the backbone of the websockets themselves checking if it well formed and recieving the full request
- RoomHandler.cs: This file defines the RoomHandler class, this is a class which defines rooms for users who are in a game to play in. It also handles the passing of the state back to the GameHandler
- GameHandler.cs: In here we define an abstract GameHandler class so if we ever want to expand into other games as well we can with ease as well as define our core Tic-Tac-Toe game. In here it will define the underlining board, initialize it and then define a Play method to handle the state passed from the front-end. This will either return a true or a false based on whether or not the board was able to move.
