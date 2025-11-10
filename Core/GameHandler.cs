using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualBasic;
using SocketHandler.Core;


/*
Prologue

Authors: Barrett Brown, Adam Berry, Alex Phibbs, Minh Vu, Jonathan Gott
Creation Date: 11/08/2025

Description:
- Defines the abstract base class for all backend game logic ('GameHandler') and the central factory responsible for instantiating game types ('GameFactory').
- Implements the TicTacToe game as an example concrete 'GameHandler'.
- The GameHandler abstraction provides a uniform interface for:
    - storing game state,
    - managing players,
    - applying moves,
    - exposing a serializable “View” of the game, and
    - reporting win/draw/playing status.
- 'GameFactory' creates the correct GameHandler based on a string 'gameKey', enabling multi-game support under a unified API.

Functions / Classes:
- enum State: Represents the game’s status ('Win', 'Draw', 'Playing').

- abstract class GameHandler:
    - View: Returns an object representing the current game state (used for frontend rendering updates).
    - GameKey: Unique string identifier for each game type. This is more future proofing but can be used in RoomHandler
      If needing to send out/handle games different in weird use cases
    - Play(state: string, client: ClientInfo): Applies a move sent from the frontend, updates state, and returns success.
    - Join(client: ClientInfo): Adds a client to the game (used for determining player order and spectators).
    - state: Indicates current win/draw/playing condition.
    - Players: Maps players to metadata such as join index.

- static class GameFactory:
    - CreateGame(gameKey: string): Returns a new GameHandler instance based on the provided key.
      Currently defaults to TicTacToe. Might want to change later if we want full proper error handling

- class TicTacToe : GameHandler:
    - Implements TicTacToe-specific game rules, board representation, and state transitions.
    - Board: 3×3 array storing the current playing grid. Currently stores as a 2D array [[],[],[]] with each entry being a "Line" on the board
    - Play(state: string, client: ClientInfo): Handles move validation, turn logic, applying updates, and triggering win/draw checks.
    - WinDetection(): Evaluates the board for terminal game states.
    - Join(client: ClientInfo): Assigns players to indices so as to manage allowed move order.

Inputs:
- Frontend JSON strings describing move attempts (via WebSocket messages).
- ClientInfo objects representing players joining rooms.
- Game key strings used by GameFactory to construct the correct game mode.

Outputs:
- Updated game state delivered through the 'View' property for broadcasting.
- Boolean results of move attempts from 'Play()'. Used to tell RoomHandler whether or not to broadcast this view.
- Updated internal player lists and game status indicators. (Not used as the moment but can be passed back with RoomHandler so as to tell frontend more if needed later)
*/



namespace Game.Core
{
	public enum State { Playing, Win, Draw }
	public abstract class GameHandler
	{
		public abstract object View { get; }
		public abstract string GameKey { get; }
		public abstract bool Play(string state, ClientInfo client);
		public abstract void Join(ClientInfo client);
		public abstract State state { get; }
		public abstract ConcurrentDictionary<Guid, int> Players { get; set; }
		public abstract int MaxPlayers { get; }

	}

	public static class GameFactory
	{
		public static GameHandler CreateGame(string gameKey)
		{
			return gameKey.ToLowerInvariant() switch
			{
				"tictactoe" => new TicTacToe(),
				_ => new TicTacToe(),// TODO: Set to break + handling; Currently set default to TicTacToe but can change to break + handle for later just set it for testing reasons
			};
		}
	}
	// Notes:
	//  Consider adding a public "Winner" field to the View so the frontend can display who won.
	//  You may also add a "NextTurn" variable to track whose move is expected.
	//  The Play() method will likely parse a JSON string (state) into key/value data.
	public class TicTacToe : GameHandler
	{
		// Description:
		//  Defines the TicTacToe game logic that inherits from GameHandler.
		//
		// Inputs:
		//    The Play() method receives a JSON string from the frontend describing a move.
		//    Example: { "event": "click", "symbol": "X" }.
		//
		// Outputs:
		//    Updates the internal board state and exposes it through the View property.
		//    Returns a bool indicating whether the move was successfully applied.
		//    The server acts as the single source of truth for game state and turn order.

		public enum Cell { Empty, X, O }

		public override string GameKey => "tictactoe"; // Added in case we need to tell the game in roomhandler down the line
		public Cell[,] Board { get; } = new Cell[3, 3]; // The board state initalized using list comprehension to a list of 9 empty cells
		private State _state = State.Playing; // Set the inital state to "playing" to signify a match has started
		public override State state => _state; // Allow other outside classes to get the state at any time while not updating it as updating will only happen within this class
    public override object View => Board;
		public override int MaxPlayers => 2; // Used as "Max PLayers playing" not spectators, allows for easy checking against index in Players dictionary and quick play open room checking

		public override ConcurrentDictionary<Guid, int> Players { get; set; } = new ConcurrentDictionary<Guid, int>(); // Used to store a dictionary of players + indexes of join order.

    private int CurrentTurn = 0;
		public override bool Play(string state, ClientInfo client)
		{
			// Description:
			//  Handles a player's move. Verifies that the move is valid (e.g., within bounds,
			//  correct turn, and not overwriting an existing cell), then applies it to the board.
			//
			// Inputs:
			//    JSON string from the frontend representing the move.
			//    The client themselves
			// Outputs:
			//    Updates the board and returns true if a valid move was made.
			//
			// TODO:
			//   Add base case validation (is game still playing, bounds check, correct turn, etc.)
			//   Apply the move once validated.

			// Console.WriteLine($"PLAY invoked by client {client.ClientID}");
			// Console.WriteLine($"Players has keys: {string.Join(',', Players.Keys)}"); 
			// make sure game is still playing
			if (_state != State.Playing) { return false; }

			// ensures only X and O can play
			if (!Players.TryGetValue(client.ClientID, out int player_index)) { return false; }

			// Must be the correct turn
			if (player_index != 0 && player_index != 1) { return false; } // 3 previous statements are for base case validations
			if (player_index != CurrentTurn) { return false; }

			// Parse thru the JSON input string
			int row, col;
			using var doc = JsonDocument.Parse(state);
			row = doc.RootElement.GetProperty("Row").GetInt32();
			col = doc.RootElement.GetProperty("Col").GetInt32();

			// check bounds
			if (row < 0 || row > 2 || col < 0 || col > 2) { return false; }

			// check to see if cell is filled
			if (Board[row, col] != Cell.Empty) { return false; }

			// Check to see if the player is player X or player O

			Board[row, col] = player_index == 0 ? Cell.X : Cell.O; // Player 0 = X, Player 1 = O 
      CurrentTurn = CurrentTurn == 0 ? 1 : 0;
			WinDetection(); // Used to update State
			return true; // If nothing stopped it/the play was made we return true since we did a move
		}

		private void WinDetection()
		{
			// Description:
			//  Checks the board for win or draw conditions and updates _state accordingly.
			//
			// Inputs: None - uses the current board state.
			// Outputs: None - updates the internal _state variable.
			//
			// Check rows and columns for a win
			for (int i = 0; i < 3; i++)
			{
				// Check row i
				if (Board[i, 0] != Cell.Empty &&
					Board[i, 0] == Board[i, 1] &&
					Board[i, 1] == Board[i, 2])
				{
					_state = State.Win;
					return;
				}

				// Check column i
				if (Board[0, i] != Cell.Empty &&
					Board[0, i] == Board[1, i] &&
					Board[1, i] == Board[2, i])
				{
					_state = State.Win;
					return;
				}
			}
			// Check diagonals
			if (Board[0, 0] != Cell.Empty &&
				Board[0, 0] == Board[1, 1] &&
				Board[1, 1] == Board[2, 2])
			{
				_state = State.Win;
				return;
			}
			if (Board[0, 2] != Cell.Empty &&
				Board[0, 2] == Board[1, 1] &&
				Board[1, 1] == Board[2, 0])
			{
				_state = State.Win;
				return;
			}
			// Check for draw (no empty cells left)
			bool hasEmpty = false;
			for (int r = 0; r < 3; r++)
			{
				for (int c = 0; c < 3; c++)
				{
					if (Board[r, c] == Cell.Empty)
					{
						hasEmpty = true;
						break;
					}
				}
				if (hasEmpty) break;
			}
			if (!hasEmpty)
			{
				_state = State.Draw;
				return;
			}

			// Otherwise, still playing
			_state = State.Playing;

		}

		public override void Join(ClientInfo client)
		{
			// Descrition:
			//  Will be called when a user joins this room use this to make a "Player 1, 2,..."
			//  and internal spectators so as to only allow certain players to play a move
			// 
			// Inputs: The client themselves
			// Ouputs: None - Updates internal "players" variable
			// 
			if (Players.ContainsKey(client.ClientID)) { return; }

			if (Players.Count < MaxPlayers)
      {
				Players.TryAdd(client.ClientID, Players.Count);
				Console.WriteLine($"Player joined: {client.ClientID} as player {Players.Count}"); 
				return; 
      }

			if (Players.Count >= MaxPlayers)
			{
				// Optional: Track spectators separately if needed
				return;
			}
		}
	}
}
