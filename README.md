# EECS 581 Project 3 (Tic-Tac-Toe Full Stack Project)

Super cool tic-tac-toe project

## Resources

-   [Websocket Support in ASP.NET Core - Basically a C# websocket getting started page](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-9.0)
-   [.Net Dependency Download the .Net SDK v9+](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
-   [Pico css documentation](https://picocss.com/docs)
-   xUnit Testing Framework:
    -   [Introduction to writing xUnit test cases](https://medium.com/bina-nusantara-it-division/a-comprehensive-guide-to-implementing-xunit-tests-in-c-net-b2eea43b48b)
    -   [Microsoft Resource on Writing test cases](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit#create-a-test)
    -   [xUnit Documentation](https://xunit.net/docs/getting-started/v2/getting-started#write-your-first-theory)

## Structure

-   `wwwroot/` is where all static files are located. This includes html, css, and js files. Something to be aware of is that every file in `wwwroot` can be served to a user so it is best practice to keep things that don't need to be there out of it.
-   `Core/` stores all of the C# files except for `Program.cs` which serves as the entry point for the app
-   `Documents/` holds all of our artifacts such as our requirements spreadsheet etc for assignments
-   `WebGames.Tests/` holds all of our unit tests and general testing files that will run upon using `dotnet test`

## Notes

-   Note at top of "Websocket Support in ASP.NET Core" page said that chrome/edge have http/2 websockets enabled by default but if you are in firefox you might have to enable it. Idk if we will/are going to use that specifically but just in case some error happens and you dont know why it is a good check

## Testing

-   Unit Tests:

    -   What is a Unit Test: A Unit Test is a test that will "do something" and then check if it is right for example call `add(1,2)` and make sure it returns 3 so if you think about it in the context of our program these allow us to verify that if we change anything that the program still runs as expected
    -   Writing One: Inside of the foldeer `/WebGames.Tests/` you will find a number of `UnitTest.cs` files. I have written one for us but these are using the xUnit framework. Feel free to follow the links at the top of the first unit test or at the top of this README to learn more about xUnit
        -   Structure Note: For now we have it all in 1 file, as this file contains just tests for "GameHandler" this is fine but if making them for other files lets have a UnitTest file for each. This will allow us to easily find/add/and edit what we need
    -   Testing: Run the command `dotnet test` and it will automatically build/run/test these tests. If there is any errors it should log them for us

-   (Dependency Test/making sure you are good to go)
    -   Starting Server: Run `dotnet run` in the folder with "Program.cs"
    -   To test using the front end html/js files open the link `http://localhost:5238` (port could differ just look in the terminal to get what it says) in the browser of your choice

## File Descriptions

-   Program.cs: Entry point of the program, it is where the basic application is compiled together, static files told to be served out, and the websocket listener starts
-   WebSocketHandler.cs: This file will be what handles the websocket requests recieved. This includes getting the client a GUID, creating a ClientInfo profile for them and passing them off to RoomHandler to work with their state. This file is mainly the backbone of the websockets themselves checking if it well formed and recieving the full request
-   RoomHandler.cs: This file defines the RoomHandler class, this is a class which defines rooms for users who are in a game to play in. It also handles the passing of the state back to the GameHandler
-   GameHandler.cs: In here we define an abstract GameHandler class so if we ever want to expand into other games as well we can with ease as well as define our core Tic-Tac-Toe game. In here it will define the underlining board, initialize it and then define a Play method to handle the state passed from the front-end. This will either return a true or a false based on whether or not the board was able to move.
