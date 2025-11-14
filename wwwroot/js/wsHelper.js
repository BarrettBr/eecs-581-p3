/*
Prologue

Authors: Barrett Brown, Adam Berry, Alex Phibbs, Minh Vu, Jonathan Gott
Creation Date: 11/08/2025

Description:
- Provides a collection of frontend utility functions for identifying the current game,
  validating/creating room IDs, reading and writing URL parameters, establishing WebSocket
  connections, and sending/receiving messages.
- Acts as the client-side helper library to ensure users enter valid multiplayer rooms
  and that the frontend reliably connects to the backend WebSocket server.
- Also contains a lightweight queuing system that stores outbound messages while the socket
  is still opening, preventing message loss during fast user interactions. This MIGHT cause issues later
  but was mainly added as a preventative measure for now

Functions:
- findGame():
    Extracts the first URL path segment (e.g., "/tictactoe/123" -> "tictactoe").
    Used to determine which game type the user is currently accessing.

- GuidValidate(id):
    Validates whether a string matches the standard GUID format (8-4-4-4-12 hex groups).

- setParam(name, value):
    Updates or inserts a URL query parameter without reloading the page.

- getParam(name):
    Retrieves a query parameter value from the URL.

- FindOrCreateRoomID():
    Gets the "roomID" parameter; if missing or invalid, generates a new UUID and updates the URL.

- connect(gameOverride):
    Establishes a WebSocket connection to the backend.
    Input:
      - Optional game override name (string)
    Behavior:
      - Determines game + roomID
      - Builds WebSocket URL with proper encoding
      - Creates the WebSocket and initializes an outbound message queue in case of still connecting
    Output:
      - Returns a connected (or connecting) WebSocket object

- send(socket, msg):
    Sends a text message or JSON payload over the WebSocket.
    Behavior:
      - If OPEN -> sends immediately
      - If CONNECTING -> stores message in socket._queue to send after onopen gets triggered
    Output:
      - boolean success/failure

- WSReceiver(socket, handler):
    Attaches an onmessage listener that automatically parses JSON messages
    and forwards them to a provided handler.

Inputs:
- URL path and query parameters
- Frontend interaction deciding which game to enter or which messages to send
- WebSocket messages from the backend. This goes to WSReceiver

Outputs:
- Updated URL parameters
- A connected WebSocket instance
- JSON-parsed messages delivered to the UI layer
*/

function findGame() {
    // Gets and returns the first segment in the url pathname i.e: returns url.com/path/two this will return the "path" bit
    const segs = location.pathname.split("/").filter(Boolean); // Used filter boolean to remove blank "" that can happen at the start of splits based on urls
    return segs[0].toLowerCase();
}

function GuidValidate(id) {
    // Regex: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/RegExp/test
    // Definition of a GUID based on googles search
    // The standard string representation of a GUID in C# consists of 32 hexadecimal digits formatted into five groups separated by hyphens, following an 8-4-4-4-12 pattern.
    if (!id) {
        return false;
    }
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
        id
    );
}

function setParam(name, value) {
    // Based on https://stackoverflow.com/questions/486896/adding-a-parameter-to-the-url-with-javascript
    const url = new URL(location.href);
    url.searchParams.set(name, value); // Can later create a "appendParam" where we change this to append if we need to have multiple values
    history.replaceState({}, "", url.toString());
}

function getParam(name) {
    // Based on https://www.sitepoint.com/get-url-parameters-with-javascript/
    return new URLSearchParams(location.search).get(name);
}

function generateUUIDv4() {
    // Based on https://www.usefulids.com/resources/generate-uuid-in-typescript
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(
        /[xy]/g,
        function (c) {
            const r = (Math.random() * 16) | 0;
            const v = c === "x" ? r : (r & 0x3) | 0x8;
            return v.toString(16);
        }
    );
}

function FindOrCreateRoomID() {
    let id = getParam("roomID");
    // If it doesn't exist/not a GUID create a new one and send it back
    if (!GuidValidate(id)) {
        id = generateUUIDv4();
        setParam("roomID", id);
    }
    return id;
}

export function connect(gameOverride) {
    const base_url = window.CONFIG?.socket_url;
    if (!base_url) {
        throw new Error("Socket url not set (wsHelper.js)");
    }

    // Get the game/room id validate them and then append them to url
    const game = gameOverride ?? findGame();
    const roomID = FindOrCreateRoomID();
    const final_url = `${base_url}?roomID=${encodeURIComponent(
        roomID
    )}&game=${encodeURIComponent(game)}`; // Used encodeURIComponent based off of this article talking about issues with special characters shouldn't matter but prevents users breaking it on purpose https://frontendmasters.com/blog/encoding-and-decoding-urls-in-javascript/

    const socket = new WebSocket(final_url);
    // After opening the socket if any "things" the user did they will be done in order now. This MIGHT cause issues later but I mainly included this to hopefully
    // prevent "slow starts" and a general feeling of waiting as the load should take <1 second
    socket._queue = [];
    socket.onopen = () => {
        while (socket._queue.length) {
            socket.send(socket._queue.shift()); // Shift is pop but from head, this loop basically just clears the queue if it exists
        }
    };
    return socket;
}

export function send(socket, msg) {
    const payload = typeof msg === "string" ? msg : JSON.stringify(msg);
    if (socket.readyState === WebSocket.OPEN) {
        socket.send(payload);
        return true;
    }
    if (socket.readyState === WebSocket.CONNECTING) {
        // Null Coalescing assignment (Used in C# side as well)
        // If queue is null set to a blank list basically way to ensure it is actually made since we use shift above it might remove the list
        socket._queue ??= [];
        socket._queue.push(payload);
        return true;
    }
    return false;
}

export function WSReceiver(socket, handler) {
    socket.onmessage = (event) => handler(JSON.parse(event.data));
}
