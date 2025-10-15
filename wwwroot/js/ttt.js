const empty = 0;
const square_x = 1;
const square_o = 2;

var canvas = document.getElementById("mainCanvas");
var ctx = canvas.getContext("2d");
var width = canvas.width;
var height = canvas.height;

var board = [
	[empty, empty, empty],
	[empty, empty, empty],
	[empty, empty, empty],
];


// Used for drawing the basic lines to the screen
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
function draw(){
	drawGrid();
	drawState();

	window.requestAnimationFrame(draw);
}

function init(){
	window.requestAnimationFrame(draw);
}

init();
