# EECS 581 Project 3 (Tic-Tac-Toe Full Stack Project)

Super cool tic-tac-toe project

## Resources

- [Websocket Support in ASP.NET Core - Basically a C# websocket getting started page](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-9.0)
- [.Net Dependency Download the .Net SDK v9+](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- (OPTIONAL) Used for testing echo reponse if you want to NOT NEEDED, and many other ways to test this. Just a "If you want to *really* be sure [NPM Node.Js Download](https://nodejs.org/en/download)

## Structure
- ```wwwroot/``` is where all static files are located. This includes html, css, and js files. Something to be aware of is that every file in ```wwwroot``` can be served to a user so it is best practice to keep things that don't need to be there out of it.
- ```Core/``` stores all of the C# files except for ```Program.cs``` which serves as the entry point for the app
- ```Documents/``` holds all of our artifacts such as our requirements spreadsheet etc for assignments

## Notes
- Note at top of "Websocket Support in ASP.NET Core" page said that chrome/edge have http/2 websockets enabled by default but if you are in firefox you might have to enable it. Idk if we will/are going to use that specifically but just in case some error happens and you dont know why it is a good check


## Testing
- (Dependency Test/making sure you are good to go) Connect to WS server/echo back information
  - Starting Server: Run ```dotnet run``` in the folder with "Program.cs"
  - To test using the front end html/js files open the link ```http://localhost:5238``` in the browser of your choice
  - (OPTIONAL) Connecting to server/echoing information: In a different terminal run ```npx wscat -c ws://localhost:5238/ws``` this is one of many ways to do it but this uses an easy npm package to test it so make sure npm is installed before this if wanting to do it. You can always look up alternate ways by just searching something along the lines of "testing local websocket server" as this is locally hosted you can't currently access this through online tools
    - After connecting just type whatever and press enter and it will echo it back to you
