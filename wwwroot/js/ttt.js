import { connect, send, WSReciever } from "./wsHelper.js";

/*
Prologue

Authors: Barrett Brown, Adam Berry, Alex Phibbs, Minh Vu, Jonathan Gott
Creation Date: 11/08/2025

Description:
- Frontend implementation of the TicTacToe game UI.  
- Responsible for drawing the game board, handling user interaction, maintaining a local
  screen-side copy of the board, and synchronizing moves with the backend WebSocket server.
- Provides click handling, canvas rendering logic, local board helpers, and imports
  a WebSocket receiver that updates the UI whenever the backend broadcasts a new view.
- Works with wsHelper.js, which handles connection setup, queuing, and message sending.

Functions / Components:
- drawGrid():
    Draws the TicTacToe grid lines on the HTML canvas.

- drawState():
    Renders X's and O's based on the 'board' 2D array. Centers each character in a cell.

- editBoard(Row, Col, state):
    Safely updates the board with X/O/empty values, updates ARIA status text for accessability,
    and triggers a redraw.

- draw():
    Clears the canvas and redraws the entire board (grid + symbols).
    Called initially and after each change to the board.

- clickHandler(event):
    Computes which cell was clicked based on pixel coordinates then sends a move request
    to the backend via 'send(socket, { Event: "move", Row, Col })'. We might want to change the event later all depends on the expected values

- windowResized():
    Updates cached canvas width/height and recalculates cell size when the browser resizes.

- init():
    Entry point for the file; draws the initial empty grid.

Sockets:
- Creates the WebSocket connection via 'connect("tictactoe")'.
- Uses WSReciever to process backend messages.
  Expected message format:
    {
        Event: "view",
        board: [ [0,1,2], ... ]
    }
- Backend view updates replace the local board and trigger a full redraw.

Inputs:
- User mouse clicks (mapped to board cell positions)
- Real-time board updates received from the backend WebSocket server
- Window resize events

Outputs:
- Updated canvas showing the current TicTacToe board state
- JSON move messages sent to the backend (Row/Col)
- ARIA status updates for accessibility
*/

// Used to represent the square states without random magic numbers
const empty = 0,
    square_x = 1,
    square_o = 2;

// These variables hold the canvas context and canvas metadata
var canvas = document.getElementById("ttt-game");
var ctx = canvas.getContext("2d");

// bind the handlers to the proper element
canvas.addEventListener("click", clickHandler);
window.addEventListener("resize", windowResized);

// These variables represent the screen state and are kept up to date by windowResized
var canvas_width = canvas.width;
var canvas_height = canvas.height;
var wcell = canvas_width / 3;
var hcell = canvas_height / 3;
var status_element = document.getElementById("game-status");

// Used to store the move state locally
// ***IMPORTANT** it is best practice to use the editBoard() function
// to edit the state in any way
// TODO: Match Board to match format of backend board or upon recieving read/update this properly (More of a note about format mismatch *could just backend that works)
var board = [
    [empty, empty, empty],
    [empty, empty, empty],
    [empty, empty, empty],
];

// Function bound to the window's onresized event
function windowResized() {
    canvas_width = canvas.width;
    canvas_height = canvas.height;
    wcell = canvas_width / 3;
    hcell = canvas_height / 3;
}

var alt = true; // this variable has no value except for demos

// This function is bound to the canvas onclick function and as expected it calculates the
// Row and Column that the user clicked on and then edits the board appropriately
// Testing Methodology: Clicked on the screen a bunch of times and it worked, even at the bounds
function clickHandler(event) {
    // Get the absolute X/Y clicked on and adjust it by the canvas' on screen position
    var bounds = canvas.getBoundingClientRect();
    const canvas_click_y = event.clientY - bounds.top;
    const canvas_click_x = event.clientX - bounds.left;

    const clicked_row = Math.floor(canvas_click_y / hcell);
    const clicked_col = Math.floor(canvas_click_x / wcell);

    // This "send" is how we send to the backend Idk what alt does so I left it for sake of not screwing you up later
    // However this is formed gamehandler will recieve/deal with so knowing the form here/backend is important
    send(socket, { Event: "move", Row: clicked_row, Col: clicked_col });
    alt = !alt;
}

// Used for drawing the basic lines to the screen
// Testing Methodology: I ran this function on load and the lines appear as expected
function drawGrid() {
    // Creating the horizontal bars
    ctx.moveTo(0, hcell);
    ctx.lineTo(canvas_width, hcell);
    ctx.stroke();

    ctx.moveTo(0, 2 * hcell);
    ctx.lineTo(canvas_width, 2 * hcell);
    ctx.stroke();

    // Creating the vertical bars
    ctx.moveTo(wcell, 0);
    ctx.lineTo(wcell, canvas_height);
    ctx.stroke();

    ctx.moveTo(2 * wcell, 0);
    ctx.lineTo(2 * wcell, canvas_height);
    ctx.stroke();
}

// This is used to draw the x's and o's on the board
// Input: The board 2d array
// Testing Methodology: tested a variety of states in the 2d array with empty, O's, and X's
function drawState() {
    ctx.font = "48px serif";
    for (var row = 0; row < board.length; row++) {
        for (var col = 0; col < board[0].length; col++) {
            let content = "";
            if (board[row][col] == square_x) {
                content = "X";
            } else if (board[row][col] == square_o) {
                content = "O";
            }
            let textWidth = ctx.measureText(content).width;
            ctx.fillText(
                content,
                col * wcell + wcell * 0.5 - textWidth / 2,
                row * hcell + hcell * 0.5 + 18
            );
        }
    }
}

// Used to add an X or an O to the board
// Will likely need to be modified later to reduce code in the socket functions
// input: the Row and Col of the square and the state to update it to
// Testing Methodology: tested using the console to call it dynamically and the board updated as intended
//						Also worked when calling it on function load
function editBoard(row, col, state) {
    // check that we are setting the square to a valid state
    if (![empty, square_x, square_o].includes(state)) {
        return;
    }
    board[row][col] = state;

    // Not strictly necessary but sends updates to an aria role for screen reader compatability
    if (state == square_x) {
        status_element.textContent = `X placed at (${row}, ${col})`;
    } else if (state == square_o) {
        status_element.textContent = `O placed at (${row}, ${col})`;
    }
    // Call the UI to update the screen
    draw();
}

// This function is called on screen load and after editBoard() is called
// it is very important this function is called after every update
function draw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    drawGrid();
    drawState();
}

// --------------- Sockets ---------------
const socket = connect("tictactoe");

socket.onclose = () => console.log("Closed connection to socket server");

WSReciever(socket, (msg) => {
    // Built this out a little to help you but this expects the backend to pass back
    // {
    //    Event: "view",
    //    Board: [[0,0,0],[0,0,0],[0,0,0]],
    //    State: 0,1,2
    // }
    // Something that might need to be checked is compatability between the backend enum version and frontend representation of cells i.e: if backend is 0,1,2 (empty, x, o) and front is 0,1,2 (empty, o, x)
    console.log("VIEW MSG:", msg); 
    if (msg.Event === "view"){
        board = msg.Value; 
        draw(); 
    }
});
// ---------------  ---------------

// The entry point of this file
function init() {
    draw();
}

init();
