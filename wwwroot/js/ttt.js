// Used to represent the square states without random magic numbers
const empty = 0;
const square_x = 1;
const square_o = 2;

// These variables hold the canvas context and canvas metadata
var canvas = document.getElementById("ttt-game");
var ctx = canvas.getContext("2d");
var width = canvas.width;
var height = canvas.height;
var status_element = document.getElementById('game-status');  

// Used to store the move state locally
var board = [
	[empty, empty, empty],
	[empty, empty, empty],
	[empty, empty, empty],
];


// Used for drawing the basic lines to the screen
// Testing Methodology: I ran this function on load and the lines appear as expected
function drawGrid(){
	let wcell = width/3;
	let hcell = height/3;
		
	// Creating the horizontal bars
	ctx.moveTo(0, hcell);
	ctx.lineTo(width, hcell);
	ctx.stroke();

	ctx.moveTo(0, 2*hcell);
	ctx.lineTo(width, 2*hcell);
	ctx.stroke();

	// Creating the vertical bars
	ctx.moveTo(wcell, 0);
	ctx.lineTo(wcell, height);
	ctx.stroke();

	ctx.moveTo(2*wcell, 0);
	ctx.lineTo(2*wcell, height);
	ctx.stroke();
}

// This is used to draw the x's and o's on the board
// Input: The board 2d array
// Testing Methodology: tested a variety of states in the 2d array with empty, O's, and X's
function drawState(){
	let wcell = width/3;
	let hcell = height/3;
	ctx.font = "48px serif";
	for(var row = 0; row < board.length; row++){
		for(var col = 0; col < board[0].length; col++){
			if(board[row][col] == square_x){
				ctx.fillText("X", (col*hcell+hcell*.5-18), (row*wcell+wcell*.5+18));
			}else if(board[row][col] == square_o){
				ctx.fillText("O", (col*hcell+hcell*.5-18), (row*wcell+wcell*.5+10));
			}
		}
	}
}

// Used to add an X or an O to the board
// Will likely need to be modified later to reduce code in the socket functions
// input: the row and col of the square and the state to update it to
// Testing Methodology: tested using the console to call it dynamically and the board updated as intended
//						Also worked when calling it on function load
function editBoard(row, col, state){
	// check that we are setting the square to a valid state
	if( ![empty, square_x, square_o].includes(state)){
		return;
	}
	board[row][col] = state;

	// Not strictly necessary but sends updates to an aria role for screen reader compatability
	if(state == square_x){
		status_element.textContent = `X placed at (${row}, ${col})`;
	} else if (state == square_o){
		status_element.textContent = `O placed at (${row}, ${col})`;
	}
}

// This function is used to draw the board state ~60 fps 
// It functions in a kind of polling way where any updates inbetween frames get caught in the next update
function draw(){
	ctx.clearRect(0,0, canvas.width, canvas.height);
	drawGrid();
	drawState();

	window.requestAnimationFrame(draw);
}

// The entry point of this file
function init(){
	window.requestAnimationFrame(draw);
}

init();
