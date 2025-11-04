// Used instead of magic numbers
const rock = 1, scissors = 2, paper = 3, none = 4;


// These variables hold the canvas context and canvas metadata
var canvas = document.getElementById("rps-game");
var ctx = canvas.getContext("2d")

const num_rounds_to_win = 3;
const ui_blue = "#0172ad"; 

var p1_wins = 0;
var p2_wins = 0;

var user_action = none;


// bind the handlers to the proper element
//canvas.addEventListener("click", clickHandler);
window.addEventListener("resize", windowResized);

// These variables represent the screen state and are kept up to date by windowResized
var canvas_width = canvas.width;
var canvas_height = canvas.height;
var status_element = document.getElementById("game-status");

const socket = new WebSocket(window.CONFIG.socket_url);


// Function bound to the window's onresized event
function windowResized() {
	const rect = canvas.getBoundingClientRect();
	canvas.width = rect.width * window.devicePixelRatio;
	canvas.height = rect.height * window.devicePixelRatio;
	ctx.scale(window.devicePixelRatio, window.devicePixelRatio);


	canvas_width = canvas.width;
	canvas_height = canvas.height;
	draw()
}

// Used for checking if an (x,y) pair are inside the canvas
//
function isInbounds(xpos, ypos, border = 0){
	return (xpos-border >=0) && (xpos+border) < canvas_width && (ypos-border) >= 0 && (ypos+border) <= canvas_width;

}

function input(action){
	// Later we will probably want to add a lock here when the play time is up
	user_action = action;
	draw();
}


// --------------- Sockets ---------------
socket.onopen = function (event) {
	console.log("Connected to Server (Socket)");
};

socket.onmessage = function (event) {
	// Will need to modify this function to read state changes appropriately
	console.log("Socket received data");
};

socket.onclose = function (event) {
	// This function likely won't need to do more than this, could maybe add a reconnecting screen
	console.log("Connected to Server lost(Socket)");
};

function sendMessage() {
	// Will need to modify this function to send data in the appropriate way after user click
}
// ----------------------------------------


function draw_circle(xpos, ypos, radius, fill){
	ctx.beginPath();
	ctx.arc(xpos, ypos, radius, 0, 2 * Math.PI);
	if(fill){
		ctx.fillStyle = ui_blue;
		ctx.fill();
	}
	ctx.lineWidth = 2;
	ctx.strokeStyle = ui_blue;
	ctx.stroke();
}

function draw(){
	ctx.clearRect(0,0, canvas_width, canvas_height);

	// all of these config variables have pixel values
	// Intended for easy tweaking
	const rad = 10;
	const hpad = 40;
	const vpad = 60;
	const spacing = 30;
	
	// Add the win tracker for the local user
	for(let x = 0; x < num_rounds_to_win; x++){
		let filled = x < p1_wins;
		draw_circle(x*spacing + hpad, vpad, rad, filled);
	}
	// Add the win tracker for their opponent
	const start_pos = canvas_width - hpad - num_rounds_to_win*spacing;
	for(let x = 0; x < num_rounds_to_win; x++){
		let filled = x < p2_wins;
		draw_circle(start_pos + x*spacing, vpad, rad, filled);
	}

	// Add the local user's choice
	// Ngl there is definitely an easier way to do this...
	// I can't be bothered rn, feel free to fix
	base_image = new Image();

	var size = 120
	let vsize = 0;
	let hsize = 0;

	if (user_action == rock){
		vsize = size*1.499;
		hsize = size;
		base_image.src = '/images/stone.png';
	} else if (user_action == paper){
		vsize = size;
		hsize = size*1.705;
		base_image.src = '/images/paper.png';
	} else if (user_action == scissors){
		vsize = size*1.3;
		hsize = size*1.3;
		base_image.src = '/images/scissors.png';
	}
	base_image.onload = function(){
		ctx.drawImage(base_image, hpad-20, vpad*3 , size, size);
	}
}


// Initialize the UI and the window size after the page loads
window.addEventListener("load", windowResized);
