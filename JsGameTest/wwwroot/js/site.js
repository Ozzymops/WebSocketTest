// WebSocket
$(document).ready(function() {
    var connection = new WebSocketManager.Connection("ws://localhost:50000/game");
    connection.enableLogging = true;

    connection.connectionMethods.onConnected = () => {

    }

    connection.connectionMethods.onDisconnected = () => {

    }

    connection.clientMethods["pingMessage"] = (socketId, message) => {
        var messageText = socketId + ": " + message;
        $('#messages').append('<li>' + messageText + '</li>');
    }

    var $messageContent = $('#messageInput');
    $messageContent.keyup(function (e) {
        if (e.keyCode == 13) {
            var message = $messageContent.val().trim();

            if (message.length == 0) {
                return false;
            }

            connection.invoke("SendMessage", connection.connectionId, message);
            $messageContent.val('');
        }
    });

    connection.start();
});