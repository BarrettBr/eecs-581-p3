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
				width: 300px;
				max-width: 300px;
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

				word-wrap: break-word;
				overflow-wrap: break-word;
				white-space: normal;
			}
			.input-area {
				display: flex;
				flex-direction: row;
				gap: 4px;
				width: 300px;
			}
			#chat-input {
				flex: 1;
				padding: 5px;
				font-size: 14px;
			}
			#send-button {
				padding: 5px 8px;
				background: #007bff;
				color: white;
				border: none;
				cursor: pointer;
			}
			#send-button:hover {
				background: #0056c7;
			}
			.msg {
				margin-bottom: 4px;
				padding: 2px 4px;
				border-radius: 4px;
				line-height: 1.2;
				font-size: 14px;
				color: black;
			}
			.player1 {
				background: #8ed6e2ff;
			}
			.player2 {
				background: #cf6a6ab6;
			}
			.spectator {
				background: #82d67bff;
				font-style: italic;
			}
			</style>
			<div class="chat">
				<div class="messages" id="chat-messages"></div>
				<div class="input-area">
					<input id="chat-input" type="text" placeholder="Type message here" />
					<button id="send-button">Send</button>
				</div>
			</div>
		`;
	}

	connectedCallback(){
		this.socket = window.__GLOBAL_SOCKET;
		this.player_index = window.__GLOBAL_PLAYER_INDEX__;
		// references to the message display and the chat input and send button
		this.msgBox = this.shadowRoot.getElementById("chat-messages");
		const input = this.shadowRoot.getElementById("chat-input");
		const button = this.shadowRoot.getElementById("send-button");

		// When send is clicked, ignore empty messages, or send non empty messages to the backend
		button.addEventListener("click", () => {
			const text = input.value.trim();
			if (text.length === 0) return; 

			this.send_chat(this.socket, this.player_index, text);
			// clear chat input box after message is sent
			input.value = "";
		});

		// when enter is pressed in chat input, message is sent (allow enter to work as button)
		input.addEventListener("keypress", (e) => {
			if (e.key === "Enter") {
				button.click(); 
			}
		}); 
	}

	// sends messages to the backend
	send_chat(socket, player_index, chat_message){
		send(socket, {
			Event: "chat",
			Chat: chat_message,
			player_index: player_index,
		});
	}

	add_message(text, fromIndex) {
		const div = document.createElement("div"); 
		div.classList.add("msg"); 

		// determine who is sending the message
		if (fromIndex === 0) {
			div.classList.add("player1");
			div.textContent = `Player ${fromIndex + 1}: ${text}`;
		} 
		else if (fromIndex === 1){
			div.classList.add("player2"); 
			div.textContent = `Player ${fromIndex + 1}: ${text}`;
		}
		else {
			div.classList.add("spectator");
			div.textContent = `Spectator: ${text}`;
		}

		// add message in the chat box
		this.msgBox.appendChild(div);
		// auto-scroll so that the most recent message is always shown
		this.msgBox.scrollTop = this.msgBox.scrollHeight; 
	}

}

customElements.define("chat-box", ChatBox);
