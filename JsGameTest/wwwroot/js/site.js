$(document).ready(function () {
    // WebSocket
    var connection = new WebSocketManager.Connection("ws://localhost:50000/game");
    connection.enableLogging = true;

    connection.connectionMethods.onConnected = () => {

    }

    connection.connectionMethods.onDisconnected = () => {

    }

    connection.clientMethods["pingMessage"] = (socketId, username, message, roomCode) => {
        if ($roomContent.val() == roomCode) {
            var messageText = username + ": " + message;
            $('#messages').append('<li>' + messageText + '</li>');
        }
    }

    connection.clientMethods["returnRoomCode"] = (socketId, roomCode) => {
        if (socketId == connection.connectionId) {
            document.getElementById("statusMessage").innerHTML = "Created room with code '" + roomCode + "' as '" + $userContent.val().trim() + "'.";
            document.getElementById("preparations").style.display = "none";
            document.getElementById("chat").style.display = "block";
            console.log("Created room with code " + roomCode);

            $roomContent.val(roomCode);
        }
    }

    connection.clientMethods["joinRoom"] = (socketId, roomCode) => {
        if (socketId == connection.connectionId) {
            document.getElementById("statusMessage").innerHTML = "Joined room with code '" + roomCode + "' as '" + $userContent.val().trim() + "'.";
            document.getElementById("preparations").style.display = "none";
            document.getElementById("chat").style.display = "block";
            console.log("Joined room with code " + roomCode);
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

        if (user.length != 0) {
            if (room.length != 0) {
                connection.invoke("JoinRoom", connection.connectionId, user, room);
            }
        }
    });

    // - Open websocket connection
    connection.start();
});