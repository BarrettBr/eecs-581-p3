import { WSReceiver } from "/js/wsHelper.js";
/*
Prologue

Authors: Barrett Brown
Creation Date: 11/09/2025

Description:
- This file is used in between the home screen and the server 
- it's expected to send the user inputs to the appropriate server functions and update/redirect from the home page as needed

Functions / Values:
- quick-play
	- This function asks the backend for the link to an open room 
	- if an empty room is present the user gets redirected, otherwise alerts them no such room exists
- set-alias-btn
	- This is the button that accepts the user's nickname, only really used for backend tracking atm

Inputs:
- State from user
	- elements with id quick-play, set-alias-btn, and a text field 

Outputs:
- Sends the necessary state to the server
- Updates the client as needed, eg redirect
*/



document.getElementById("quick-play").addEventListener("click", () => {
    console.log("Hit home.js click");
    const base = window.CONFIG?.socket_url;
    const url = `${base}?quickPlayJoin=true`;
    const socket = new WebSocket(url);

    console.log("Grabbed URLS");
    WSReceiver(socket, (msg) => {
        if (msg.Event === "quickPlayJoined") {
            // Redirect user to the game route with the roomID
            const game = msg.GameKey;
            const roomID = msg.RoomID;
            location.href = `/html/${game}.html?roomID=${encodeURIComponent(
                roomID
            )}`;
        } else if (msg.Event === "nofreerooms") {
            alert("No open rooms right now"); // Could change this later just chose the basic popup for now
            socket.close();
        } else if (msg.Event === "error") {
            console.warn("WS error:", msg.Message);
        }
    });

    socket.onclose = () => console.log("Quick play socket closed");
});

document.getElementById("set-alias-btn").addEventListener("click", () => {
    const aliasInput = document.getElementById("alias-input");
    const alias = aliasInput.value.trim();
    if (alias.length === 0) {
        alert("Alias cannot be empty.");
        return;
    }
    window.CONFIG.player_alias = alias;
    console.log(`Alias set to: ${alias}`);
    aliasInput.value = "";
});
