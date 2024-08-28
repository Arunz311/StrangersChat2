"use strict";

// Create a connection to the SignalR hub
const connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();

// Start the connection and handle potential errors
connection.start().then(function () {
    console.log("Connected to hub");

    // Automatically start a chat when connected
    connection.invoke("StartChat").catch(function (err) {
        console.error("Error starting chat: " + err.toString());
    });
}).catch(function (err) {
    console.error("Error connecting to hub: " + err.toString());
    // Optionally, provide UI feedback to the user here
});

// Handle incoming messages
connection.on("ReceiveMessage", function (message) {
    const msgDiv = document.createElement("div");
    msgDiv.classList.add("message", "received");

    const msgContent = document.createElement("div");
    msgContent.classList.add("message-content");
    msgContent.textContent = message;

    msgDiv.appendChild(msgContent);
    document.getElementById("messagesList").appendChild(msgDiv);
});

// Handle successful connection to a partner
connection.on("ConnectedToPartner", function (partnerId) {
    console.log("Connected to partner: " + partnerId);
    const msgDiv = document.createElement("div");
    msgDiv.classList.add("message", "received");

    const msgContent = document.createElement("div");
    msgContent.classList.add("message-content");
    msgContent.textContent = "Connected to a stranger.";

    msgDiv.appendChild(msgContent);
    document.getElementById("messagesList").appendChild(msgDiv);
});

// Handle partner disconnection
connection.on("PartnerDisconnected", function () {
    const msgDiv = document.createElement("div");
    msgDiv.classList.add("message", "received");

    const msgContent = document.createElement("div");
    msgContent.classList.add("message-content");
    msgContent.textContent = "Your partner has disconnected.";

    msgDiv.appendChild(msgContent);
    document.getElementById("messagesList").appendChild(msgDiv);
    // Optionally, handle UI updates or reconnect logic
});

// Handle user disconnection
connection.on("YouDisconnected", function () {
    const msgDiv = document.createElement("div");
    msgDiv.classList.add("message", "received");

    const msgContent = document.createElement("div");
    msgContent.classList.add("message-content");
    msgContent.textContent = "You have ended the chat.";

    msgDiv.appendChild(msgContent);
    document.getElementById("messagesList").appendChild(msgDiv);
    // Optionally, handle UI updates or reconnect logic
});

// Handle the "No Available Partners" scenario
connection.on("NoAvailablePartners", function () {
    const msgDiv = document.createElement("div");
    msgDiv.classList.add("message", "received");

    const msgContent = document.createElement("div");
    msgContent.classList.add("message-content");
    msgContent.textContent = "No partners available at the moment.";

    msgDiv.appendChild(msgContent);
    document.getElementById("messagesList").appendChild(msgDiv);
    // Optionally, provide retry options or handle the UI
});

// Send a message to the chat
document.getElementById("sendButton").addEventListener("click", function (event) {
    const message = document.getElementById("messageInput").value;
    if (message.trim() !== "") {
        const msgDiv = document.createElement("div");
        msgDiv.classList.add("message", "sent");

        const msgContent = document.createElement("div");
        msgContent.classList.add("message-content");
        msgContent.textContent = message;

        msgDiv.appendChild(msgContent);
        document.getElementById("messagesList").appendChild(msgDiv);

        connection.invoke("SendMessage", message).catch(function (err) {
            console.error("Error sending message: " + err.toString());
            // Optionally, provide feedback to the user
        });

        document.getElementById("messageInput").value = "";
    }
    event.preventDefault();
});

// End the current chat session
document.getElementById("endChatButton").addEventListener("click", function (event) {
    connection.invoke("EndChat").catch(function (err) {
        console.error("Error ending chat: " + err.toString());
        // Optionally, provide feedback to the user
    });

    event.preventDefault();
});

// Refresh the chat and look for new partners
document.getElementById("refreshButton").addEventListener("click", function (event) {
    connection.invoke("RefreshChat").catch(function (err) {
        console.error("Error refreshing chat: " + err.toString());
        // Optionally, provide feedback to the user
    });

    event.preventDefault();
});
