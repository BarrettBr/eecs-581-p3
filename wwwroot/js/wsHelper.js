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

function FindOrCreateRoomID() {
    let id = getParam("roomID");
    // If it doesn't exist/not a GUID create a new one and send it back
    if (!GuidValidate(id)) {
        id = crypto.randomUUID();
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

export function WSReciever(socket, handler) {
    socket.onmessage = (event) => handler(JSON.parse(event.data));
}
