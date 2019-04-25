﻿$(document).ready(function () {
    // WebSocket
    var connection = new WebSocketManager.Connection("ws://localhost:50000/game");
    connection.enableLogging = false;

    connection.connectionMethods.onConnected = () => {
        repeatRoomCount();
    }

    connection.connectionMethods.onDisconnected = () => {

    }

    connection.clientMethods["pingMessage"] = (socketId, username, message, roomCode) => {
        if ($roomContent.val() == roomCode) {
            var messageText = username + ": " + message;
            $('#messages').append('<li>' + messageText + '</li>');
        }
    }

    connection.clientMethods["serverMessage"] = (message, roomCode) => {
        if ($roomContent.val() == roomCode) {
            var messageText = message;
            $('#messages').append('<li>' + messageText + '</li>');
        }
    }

    connection.clientMethods["returnRoomCode"] = (socketId, roomCode) => {
        if (socketId == connection.connectionId) {
            document.getElementById("statusMessage").innerHTML = "Created room with code '" + roomCode + "' as '" + $userContent.val().trim() + "'. You are the owner.";
            document.getElementById("preparations").style.display = "none";
            document.getElementById("chat").style.display = "block";

            $roomContent.val(roomCode);
            repeatUserList();

            var message = "[User " + $userContent.val().trim() + " joined the room.]"
            var room = $roomContent.val().trim();
            connection.invoke("ServerMessage", message, room);

            // Set host buttons
            document.getElementById("startButton").style.display = "block";
        }
    }

    connection.clientMethods["joinRoom"] = (socketId, roomCode) => {
        if (socketId == connection.connectionId) {
            document.getElementById("statusMessage").innerHTML = "Joined room with code '" + roomCode + "' as '" + $userContent.val().trim() + "'.";
            document.getElementById("preparations").style.display = "none";
            document.getElementById("chat").style.display = "block";

            repeatUserList();
        }
    }

    connection.clientMethods["leaveRoom"] = (socketId) => {
        if (socketId == connection.connectionId) {
            document.getElementById("statusMessage").innerHTML = "Left room.";
            document.getElementById("preparations").style.display = "flex";
            document.getElementById("chat").style.display = "none";
            console.log("Left room.");

            clearTimeout(userTimer);
            userTimer = 0;

            $('#messages').empty();
        }
    }

    connection.clientMethods["retrieveRoomCount"] = (roomCount) => {
        document.getElementById("rooms").innerHTML = "Kamers online: " + roomCount;
    }

    connection.clientMethods["retrieveUserList"] = (roomCode, ownerId, userList) => {
        if ($roomContent.val() == roomCode) {

            var users = JSON.parse(userList);

            $('#users').empty();

            for (var x in users)
            {
                var tempString = users[x];
                var tempArray = tempString.split(':|!');

                if (ownerId == connection.connectionId) {
                    // var idString = "'" + tempArray[1] + "'";
                    // $('#users').append('<li>' + tempArray[0] + '<input class="form-check" onClick="$.kickUser(' + idString + ')" type="button" value="Kick" />' + '</li>');
                    var idString = "'" + tempString + "'";
                    $('#users').append('<li>' + tempArray[0] + '<input class="form-check" onClick="$.kickUser(' + idString + ')" type="button" value="Kick" />' + '</li>');
                }
                else {
                    $('#users').append('<li>' + tempArray[0] + '</li>');
                }
            }
        }
    }

    // Functions
    var $messageContent = $('#messageInput');
    var $userContent = $('#usernameInput');
    var $roomContent = $('#roomInput');

    // - Send message on 'enter'
    $messageContent.keyup(function (e) {
        if (e.keyCode == 13) {
            var message = $messageContent.val().trim();
            var user = $userContent.val().trim();
            var room = $roomContent.val().trim();

            if (user.length != 0) {
                if (room.length != 0) {
                    if (message.length != 0) {
                        connection.invoke("SendMessage", connection.connectionId, user, message, room);
                    }
                }
            }

            $messageContent.val('');
        }
    });

    // - Create room
    $('#createButton').click(function () {
        var user = $userContent.val().trim();

        if (user.length != 0) {
            connection.invoke("CreateRoom", connection.connectionId, user);
        }
    });

    // - Join room
    $('#joinButton').click(function () {
        var user = $userContent.val().trim();
        var room = $roomContent.val().trim();
        var message = "[User " + $userContent.val().trim() + " joined the room.]";

        if (user.length != 0) {
            if (room.length != 0) {
                connection.invoke("JoinRoom", connection.connectionId, user, room);
                connection.invoke("ServerMessage", message, room);
            }
        }
    });

    // - Leave room
    $('#leaveButton').click(function () {
        var room = $roomContent.val().trim();
        var message = "[User " + $userContent.val().trim() + " left the room.]";

        if (room.length != 0) {
            connection.invoke("ServerMessage", message, room);
            connection.invoke("LeaveRoom", connection.connectionId, room);

            $userContent.val('');
            $messageContent.val('');
            $roomContent.val('');
        }
    });

    // - Kick user from lobby
    $.kickUser = function (user) {
        var room = $roomContent.val().trim();
        var tempArray = user.split(":|!");
        var message = "[User " + tempArray[0] + " was kicked.]";

        if (room.length != 0) {
            connection.invoke("ServerMessage", message, room);
            connection.invoke("LeaveRoom", tempArray[1], room);
        }
    }

    function repeatRoomCount() {
        connection.invoke("RetrieveRoomCount");
        setTimeout(repeatRoomCount, 1000);
    }

    function repeatUserList() {
        var room = $roomContent.val().trim();

        if (room.length != 0) {
            connection.invoke("RetrieveUserList", room);
            userTimer = setTimeout(repeatUserList, 1000);
        }
    }

    // - Open websocket connection
    connection.start();
});