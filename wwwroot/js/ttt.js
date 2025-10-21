// Used to represent the square states without random magic numbers
const empty = 0, square_x = 1, square_o = 2;

// These variables hold the canvas context and canvas metadata
var canvas = document.getElementById("ttt-game");
var ctx = canvas.getContext("2d");

// bind the handlers to the element
canvas.addEventListener("click", clickHandler);
window.addEventListener("resize", windowResized); 

var canvas_width = canvas.width;
var canvas_height = canvas.height;
var wcell = canvas_width/3;
var hcell = canvas_height/3;
var status_element = document.getElementById('game-status');  

// Used to store the move state locally
// ***IMPORTANT** it is best practice to use the editBoard() function
// to edit the state in any way
var board = [
	[empty, empty, empty],
	[empty, empty, empty],
	[empty, empty, empty],
];

// Function bound to the window's onresized event
function windowResized(){
	canvas_width = canvas.width;
	canvas_height = canvas.height;
	wcell = canvas_width/3;
	hcell = canvas_height/3;
}

var alt = true; // this variable has no value except for demos

// This function is bound to the canvas onclick function and as expected it calculates the
// row and column that the user clicked on and then edits the board appropriately
// Testing Methodology: Clicked on the screen a bunch of times and it worked, even at the bounds
function clickHandler(event){
	// Get the absolute X/Y clicked on and adjust it by the canvas' on screen position
	var bounds = canvas.getBoundingClientRect();
	canvas_click_y = event.clientY - bounds.top;
	canvas_click_x = event.clientX - bounds.left;

	clicked_row = Math.floor(canvas_click_y/hcell);
	clicked_col = Math.floor(canvas_click_x/wcell); 

	// This is all BS used for demoing
	if(alt){
		editBoard(clicked_row, clicked_col, square_x);
	} else { 
		editBoard(clicked_row, clicked_col, square_o);
	}
	alt = !alt;
}


// Used for drawing the basic lines to the screen
// Testing Methodology: I ran this function on load and the lines appear as expected
function drawGrid(){
	// Creating the horizontal bars
	ctx.moveTo(0, hcell);
	ctx.lineTo(canvas_width, hcell);
	ctx.stroke();

	ctx.moveTo(0, 2*hcell);
	ctx.lineTo(canvas_width, 2*hcell);
	ctx.stroke();

	// Creating the vertical bars
	ctx.moveTo(wcell, 0);
	ctx.lineTo(wcell, canvas_height);
	ctx.stroke();

	ctx.moveTo(2*wcell, 0);
	ctx.lineTo(2*wcell, canvas_height);
	ctx.stroke();
}

// This is used to draw the x's and o's on the board
// Input: The board 2d array
// Testing Methodology: tested a variety of states in the 2d array with empty, O's, and X's
function drawState(){
	ctx.font = "48px serif";
	for(var row = 0; row < board.length; row++){
		for(var col = 0; col < board[0].length; col++){
			let content = "";
			if(board[row][col] == square_x){
				content = "X";
			}else if(board[row][col] == square_o){
				content = "O";
			} 
			let textWidth = ctx.measureText(content).width;
			ctx.fillText(content, (col*wcell+wcell*.5 - textWidth/2), (row*hcell+hcell*.5+18));
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
	draw();
}

// This function is called on screen load and after editBoard() is called
// it is very important this function is called after every update
function draw(){
	ctx.clearRect(0,0, canvas.width, canvas.height);
	drawGrid();
	drawState();
}

// The entry point of this file
function init(){
	draw();
}

init();
