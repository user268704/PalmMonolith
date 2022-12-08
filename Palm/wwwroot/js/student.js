"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/api/signalr/session").build();

// Создание метода который будет вызываться с сервера
connection.on("StartSession", function () {
    const li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    
    li.textContent = "Session started";
});

connection.start().then(function () {
    const list = document.getElementById("messagesList");
    
    const listItem = document.createElement("li");
    list.appendChild(listItem);
    listItem.textContent = "Connection started";

    const urlPath = window.location.pathname;
    let sessionId = urlPath.substring(urlPath.lastIndexOf("/") + 1);
    
    connection.invoke("JoinSession", sessionId)
        .catch(function (err) {
            return console.error(err.toString());
        })
    
}).catch(function (err) {
    return console.error(err.toString());
});