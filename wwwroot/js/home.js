import { WSReceiver } from "/js/wsHelper.js";

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
        } else if (msg.Event === "nofree") {
            alert("No open rooms right now"); // Could change this later just chose the basic popup for now
            socket.close();
        } else if (msg.Event === "error") {
            console.warn("WS error:", msg.Message);
        }
    });

    socket.onclose = () => console.log("Quick play socket closed");
});
