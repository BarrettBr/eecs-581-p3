/*
Prologue

Authors: Barrett Brown, Adam Berry, Alex Phibbs, Minh Vu, Jonathan Gott
Creation Date: 11/08/2025

Description:
- Defines global frontend configuration values used across the client-side game pages.
- Dynamically decides the WebSocket protocol based on how the page was accessed
  (HTTP -> ws://, HTTPS -> wss://). This is used in case in the future we swap to https we don't have errors
- Exposes the configuration via the global 'window.CONFIG' object.

Functions / Values:
- protocol:
    Determines either "ws" or "wss" based on whether 'location.protocol' is http or https.

- host:
    The current domain/port where the frontend is served. (Basically the localhost:port bit)

- window.CONFIG:
  {
    socket_url: Full WebSocket base endpoint (ex: ws://localhost:5000/ws)
  }

Inputs:
- Browser location information (protocol, host)

Outputs:
- Global configuration object
*/

const protocol = location.protocol === "https:" ? "wss" : "ws"; // Changed it to be dynamic based on how thery accessed it, this shouldn't be needed since we run http but nice for later
const host = location.host;
window.CONFIG = {
    socket_url: `${protocol}://${host}/ws`,
};
