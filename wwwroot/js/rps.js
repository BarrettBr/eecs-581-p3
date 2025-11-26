import { connect, send, WSReceiver } from "/js/wsHelper.js";
/*
Prologue

Authors: Adam Berry 
Creation Date: 11/08/2025

Description:
- Serves as the intermediary between the User and the backend
- Pipes user input to the server and updates the interface when the server sends an update

Inputs:
- DOM information 
- Events (such as onClick{)

Outputs:
- Canvas/State updates
*/

// Used instead of magic numbers
const rock = 0,
    paper = 1,
    scissors = 2,
    unset = 3,
	set = 4;
const Moves = { rock, paper, scissors, unset, set}; // used to convert the input to the enum style
var socket;

// These variables hold the canvas context and canvas metadata
var canvas = document.getElementById("rps-game");
var ctx = canvas.getContext("2d");
const num_rounds_to_win = 3;
const ui_blue = "#0172ad";

var p1_wins = 0;
var p2_wins = 0;

var p1_choice = unset;
var p2_choice = unset;

var round_num = 0;

var win_amt = 3;
let cur_state;
let player_index = null;

// These variables represent the screen state and are kept up to date by windowResized
var canvas_width = canvas.width;
var canvas_height = canvas.height;

// Function bound to the window's onresized event
function windowResized() {
    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width * window.devicePixelRatio;
    canvas.height = rect.height * window.devicePixelRatio;
    ctx.scale(window.devicePixelRatio, window.devicePixelRatio);

    canvas_width = canvas.width;
    canvas_height = canvas.height;
    draw();
}

// Used for checking if an (x,y) pair are inside the canvas
//
function isInbounds(xpos, ypos, border = 0) {
    return (
        xpos - border >= 0 &&
        xpos + border < canvas_width &&
        ypos - border >= 0 &&
        ypos + border <= canvas_width
    );
}

function input(action) {
    // Later we will probably want to add a lock here when the play time is up
    var obj = new Object();
    obj.Event = "move";
    if (!(action in Moves)) {
        console.log("Input: Invalid action");
        return;
    }
    obj.selected_move = Moves[action];

    var jsonString = JSON.stringify(obj);

    send(socket, jsonString);

    draw();
}

function draw_circle(xpos, ypos, radius, fill) {
    ctx.beginPath();
    ctx.arc(xpos, ypos, radius, 0, 2 * Math.PI);
    if (fill) {
        ctx.fillStyle = ui_blue;
        ctx.fill();
    }
    ctx.lineWidth = 2;
    ctx.strokeStyle = ui_blue;
    ctx.stroke();
}

async function draw(isRoundEnd = false, override_p1=null, override_p2=null) {
    ctx.clearRect(0, 0, canvas_width, canvas_height);

    // all of these config variables have pixel values
    // Intended for easy tweaking
    const rad = 10;
    const hpad = 40;
    const vpad = 60;
    const spacing = 30;
	let local_wins = 0;
	let remote_wins = 0;
	let local_choice = unset;
	let remote_choice = unset;

	// Check if we are the "first" player
	// Also decide what we should show to the user
	if(player_index == 0){
		local_wins = p1_wins;
		remote_wins = p2_wins;
		local_choice  = override_p1 ?? p1_choice;
		remote_choice = override_p2 ?? p2_choice;
	} else {
		local_wins = p2_wins;
		remote_wins = p1_wins;
		local_choice  = override_p2 ?? p2_choice;
		remote_choice = override_p1 ?? p1_choice;
	}

	// ------------- Set Win Trackers -------------
	//
    // Add the win tracker for the local user
    for (let x = 0; x < num_rounds_to_win; x++) {
        let filled = x < local_wins;
        draw_circle(x * spacing + hpad, vpad, rad, filled);
    }

    // Add the win tracker for their opponent
    const start_pos = canvas_width - hpad - num_rounds_to_win * spacing;
    for (let x = 0; x < num_rounds_to_win; x++) {
        let filled = x < remote_wins;
        draw_circle(start_pos + x * spacing, vpad, rad, filled);
    }

    let local_image = new Image();
    let remote_image = new Image();

    var size = 120;
    let vsize = 0;
    let hsize = 0;

	// -------------  set the image for the local player -------------
	//
	// local_choice is set off the more "important" p1_choice and p2_choice
    console.log(local_choice);
	if (local_choice == rock) {
		vsize = size;
		hsize = size * 1.499;
		local_image.src = "/images/stone.png";
	} else if (local_choice == paper) {
		vsize = size * 1.605;
	 	hsize = size;
		local_image.src = "/images/paper.png";
	} else if (local_choice == scissors) {
		vsize = size * 1.3;
		hsize = size * 1.3;
		local_image.src = "/images/scissors.png";
	} else {
		// for now this isn't defined but we could add a placeholder
		return;
	}

	// -------------  set the image for the "remote" player -------------
	//
	if (remote_choice == rock) {
		vsize = size;
		hsize = size * 1.499;
		remote_image.src = "/images/stone.png";
	} else if (remote_choice == paper) {
		vsize = size * 1.605;
	 	hsize = size;
		remote_image.src = "/images/paper.png";
	} else if (remote_choice == scissors) {
		vsize = size * 1.3;
		hsize = size * 1.3;
		remote_image.src = "/images/scissors.png";
	} else {
		// for now this isn't defined but we could add a placeholder
		return;
	}

	if(isRoundEnd){
		local_image.onload = function () {
			ctx.drawImage(local_image, hpad - 20, vpad * 3, vsize, hsize);
		};

		remote_image.onload = function () {
			ctx.drawImage(remote_image, canvas_width - hsize - hpad, vpad * 3, vsize, hsize);
		};
		await new Promise(r => setTimeout(r, 2000));

		draw();
	} else {
		local_image.onload = function () {
			ctx.drawImage(local_image, hpad - 20, vpad * 3, vsize, hsize);
		};

	}
}

function initFunction() {
    // Initialize the UI and the window size after the page loads
    window.addEventListener("load", windowResized);

    // bind the handlers to the proper element
    //canvas.addEventListener("click", clickHandler);
    window.addEventListener("resize", windowResized);

    document
        .querySelectorAll("button[data-choice]")
        .forEach((btn) =>
            btn.addEventListener("click", (e) => input(e.target.dataset.choice))
        );

    socket = connect("rockpaperscissors");
    socket.onclose = () => console.log("Closed connection to socket server");

    WSReceiver(socket, (msg) => {
        // Built this out a little to help you but this expects the backend to pass back
        // { event: "view", board: 2D array}
        // Something that might need to be checked is compatability between the backend enum version and frontend representation of cells i.e: if backend is 0,1,2 (empty, x, o) and front is 0,1,2 (empty, o, x)
        try {
            if (typeof msg.Player_Index === "number") {
                player_index = msg.Player_Index;
                console.log("Update player index to: ", player_index);
            }
            if (msg?.Event === "view") {
                const StateText = ["Playing", "Win", "Draw"];
                cur_state = StateText[msg.State] ?? "Playing"; // safe fallback
                console.log(msg);

                const value = msg.Value;
                p1_wins = Number(value.Player1Wins);
                p2_wins = Number(value.Player2Wins);
                win_amt = Number(value.WinAmt);

				let newRound = Number(value.RoundNum);
				if(round_num < newRound){
					round_num = newRound; 
					// Call the draw function such that round end is set
					draw(true, Number(value.Last_P1), Number(value.Last_P2));
				}
				else{
					p1_choice = Number(value.p1_move);
					p2_choice = Number(value.p2_move);
					draw();
				}
				console.log("Choices:", p1_choice, " p2:", p2_choice);
            }
        } catch (e) {
            console.warn("Bad WS sent from Server -> client", e, msg);
        }
    });

    draw();
}

document.addEventListener("DOMContentLoaded", () => {
    console.log("Page loaded");
    initFunction();
});
