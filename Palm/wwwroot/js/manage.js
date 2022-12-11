"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/api/signalr/session").build();

//Disable the send button until connection is established.
document.getElementById("startSession").disabled = true;
document.getElementById("expelStudentButton")
    .addEventListener("click", function (event) {
        const studentId = document.getElementById("userId").value;

        connection.invoke("ExpelStudent", studentId)
            .catch(function (err) {
                return console.error(err.toString());
            });

        event.preventDefault();
    })

// Создание метода который будет вызываться с сервера
connection.on("StartSession", function () {
    const li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `Session started`;
});

connection.on("UserJoined", function (user) {
    const li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    li.textContent = `${user.userName} joined the session`;
})

connection.on("UserLeft", function (user) {
    const li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);

    li.textContent = `${user.userName} left the session`;
})

connection.on("UserDisconnect", function (user) {

    const li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);

    li.textContent = `${user.userName} disconnected`;
})

connection.on("Ping", function () {
    connection.invoke("Pong")
        .catch(function (err) {
            return console.error(err.toString());
        });
})

connection.start().then(function () {
    document.getElementById("startSession").disabled = false;
    // Get session from url query string
    const urlPath = window.location.pathname;
    let sessionId = urlPath.substring(urlPath.indexOf("/"), urlPath.lastIndexOf('/'));
    sessionId = sessionId.substring(sessionId.lastIndexOf('/') + 1);

    connection.invoke("InitialSession", sessionId)
        .catch(function (err) {
            return console.error(err.toString());
        })

}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("endSession").addEventListener("click", function (event) {
    // Get session from url query string
    const urlPath = window.location.pathname;
    let sessionId = urlPath.substring(urlPath.indexOf("/"), urlPath.lastIndexOf('/'));
    sessionId = sessionId.substring(sessionId.lastIndexOf('/') + 1);

    connection.invoke("EndSession", sessionId)
        .catch(function (err) {
            return console.error(err.toString());
        });
})

document.getElementById("startSession").addEventListener("click", function (event) {
    const user = document.getElementById("sessionId").value;

    connection.invoke("StartSession", user).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});