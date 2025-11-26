import { send } from "/js/wsHelper.js";
/*
Prologue customElement

Authors: Adam Berry
Creation Date: 11/25/2025

Description:
- Simple chat panel custom element using shadow DOM
- Displays messages and allows sending chat to the server

Functions / Components:
- constructor(): sets up shadow DOM and basic styles
- send_chat(socket, player_index, chat_message): sends chat messages via WebSocket

Inputs:
- WebSocket object (socket)
- Player index
- Chat message string

Outputs:
- Sends { Event: "chat", Chat: ..., player_index: ... } to the server
- Renders messages inside the shadow DOM

*/



class ChatBox extends HTMLElement {
	constructor() {
		super();
		this.attachShadow({ mode: "open" });
		this.shadowRoot.innerHTML = `
			<style>
			.chat {
				padding: 20px;
				width: 150;
				height: 450px;
				display: flex;
				flex-direction: column;
			}
			.messages {
				flex: 1;
				overflow-y: auto;
				margin-bottom: 10px;
				border: 1px solid #eee;
				padding: 5px;
			}
			.input{
			}
			</style>
			<div class="chat">
				<div class="messages">
					<p> fill out chat here </p>
				</div>
				<div class="input">
				</div>
			</div>
		`;
	}
	send_chat(socket, player_index, chat_message){
		send(socket, {
			Event: "chat",
			Chat: chat_message,
			player_index: player_index,
		});
	}

}

customElements.define("chat-box", ChatBox);
