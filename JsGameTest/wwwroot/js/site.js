$(document).ready(function () {
    // WebSocket
    var connection = new WebSocketManager.Connection("ws://localhost:50000/game");
    connection.enableLogging = false;

    connection.connectionMethods.onConnected = () => {
        getRoomCount();
    }

    connection.connectionMethods.onDisconnected = () => {

    }

    // Set status message
    connection.clientMethods["setStateMessage"] = (socketId, message) => {
        if (socketId == connection.connectionId) {
            document.getElementById("statusMessage").innerHTML = message;
        }
    }

    // DEBUG check room state
    connection.clientMethods["checkRoomState"] = (roomCode, state) => {
        if ($roomContent.val() == roomCode) {
            document.getElementById("roomState").innerHTML = state;
        }
    }

    // Print message inside of chat
    connection.clientMethods["pingMessage"] = (username, message, roomCode) => {
        if ($roomContent.val() == roomCode) {
            var messageText = username + ": " + message;
            $('#messages').append('<li>' + messageText + '</li>');
        }
    }

    // Print server message inside of chat
    connection.clientMethods["serverMessage"] = (message, roomCode) => {
        if ($roomContent.val() == roomCode) {
            var messageText = message;
            $('#messages').append('<li>' + messageText + '</li>');
        }
    }

    // Open up a room.
    connection.clientMethods["returnRoomCode"] = (socketId, roomCode) => {
        if (socketId == connection.connectionId) {
            inRoom = true;

            $roomContent.val(roomCode);

            document.getElementById("statusMessage").innerHTML = "Kamer aangemaakt met code '" + roomCode + "' als '" + $userContent.val().trim() + "'. Jij bent de eigenaar van de kamer.";
            document.getElementById("preparations").style.display = "none";
            document.getElementById("chat").style.display = "block";
            document.getElementById("roomState").style.display = "block";

            // Set host buttons
            document.getElementById("startButton").style.display = "block";
        }
    }

    // Join a open room.
    connection.clientMethods["joinRoom"] = (socketId, roomCode) => {
        if (socketId == connection.connectionId) {
            inRoom = true;

            document.getElementById("statusMessage").innerHTML = "Kamer met code '" + roomCode + "' ingegaan als '" + $userContent.val().trim() + "'.";
            document.getElementById("preparations").style.display = "none";
            document.getElementById("chat").style.display = "block";
            document.getElementById("roomState").style.display = "block";

            // Set host buttons
            document.getElementById("startButton").style.display = "none";
        }
    }

    // Leave a room.
    connection.clientMethods["leaveRoom"] = (socketId, kicked) => {
        if (socketId == connection.connectionId) {
            inRoom = false;

            if (kicked) {
                document.getElementById("statusMessage").innerHTML = "Je bent door de eigenaar van de kamer verwijderd.";
            }
            else {
                document.getElementById("statusMessage").innerHTML = "Je hebt de kamer verlaten.";
            }

            // Reset screen
            $('#messages').empty();
            $roomContent.val('');
            document.getElementById("preparations").style.display = "flex";
            document.getElementById("chat").style.display = "none";
            document.getElementById("roomState").style.display = "none";
        }
    }

    // Get room count
    connection.clientMethods["retrieveRoomCount"] = (roomCount) => {
        document.getElementById("rooms").innerHTML = "Kamers online: " + roomCount;
    }

    // Get list of users inside of room
    connection.clientMethods["retrieveUserList"] = (roomCode, ownerId, userList) => {
        if ($roomContent.val() == roomCode) {

            var users = JSON.parse(userList);

            $('#users').empty();

            for (var x in users)
            {
                var tempString = users[x];
                var tempArray = tempString.split(':|!');

                if (ownerId == connection.connectionId) {
                    if (tempArray[1] == connection.connectionId) {
                        $('#users').append('<li>' + tempArray[0] + '</li>');
                    }
                    else {
                        var idString = "'" + tempString + "'";
                        $('#users').append('<li>' + tempArray[0] + '<input class="form-check" onClick="$.kickUser(' + idString + ')" type="button" value="Kick" />' + '</li>');
                    }                 
                }
                else {
                    $('#users').append('<li>' + tempArray[0] + '</li>');
                }
            }
        }
    }

    // Start game with current room
    connection.clientMethods["startGame"] = (roomCode, ownerId) => {
        if ($roomContent.val() == roomCode) {
            document.getElementById("statusMessage").innerHTML = "Started game with room '" + $roomContent.val() + "'.";
            document.getElementById("preparations").style.display = "none";
            document.getElementById("chat").style.display = "none";
            document.getElementById("game").style.display = "block";

            // If owner
            if (ownerId == connection.connectionId) {
                document.getElementById("game-owner").style.display = "block";
            }
            // If client
            else {
                document.getElementById("game-client").style.display = "block";
            }
        }
    }

    // ------------------------------------------------------------------------- //
    // Functions
    var $messageContent = $('#messageInput');
    var $userContent = $('#usernameInput');
    var $roomContent = $('#roomInput');
    var inRoom = false;

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
            connection.invoke("LeaveRoom", connection.connectionId, room, false);

            $userContent.val('');
            $messageContent.val('');
            $roomContent.val('');
        }
    });

    // - Start game with current room
    $('#startButton').click(function () {
        var room = $roomContent.val().trim();

        if (room.length != 0) {
            connection.invoke("StartGame", connection.connectionId, room);

            $messageContent.val('');
        }
    });

    // - Kick user from lobby
    $.kickUser = function (user) {
        var room = $roomContent.val().trim();
        var tempArray = user.split(":|!");
        var message = "[User " + tempArray[0] + " was kicked.]";

        if (room.length != 0) {
            connection.invoke("ServerMessage", message, room);
            connection.invoke("LeaveRoom", tempArray[1], room, true);
        }
    }

    // - Get stuff every five seconds
    function repeatStuff() {
        connection.invoke("RetrieveRoomCount");

        setTimeout(repeatStuff, 5000); // repeat every five seconds
    }

    // - Get amount of online rooms
    function getRoomCount() {
        connection.invoke("RetrieveRoomCount");
    }

    // - Get current room state
    function getRoomState() {
        if (inRoom) {
            var room = $roomContent.val().trim();
            connection.invoke("CheckRoomState", room);
        }
    }

    // - Open websocket connection
    connection.start();
});