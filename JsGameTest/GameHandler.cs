using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketManager;

namespace JsGameTest
{
    public class GameHandler : WebSocketHandler
    {
        private readonly GameManager _gameManager;
        public GameHandler (WebSocketConnectionManager webSocketConnectionManager, GameManager gameManager ): base(webSocketConnectionManager)
        {
            _gameManager = gameManager;
        }

        public async Task SendMessage(string socketId, string message)
        {
            dynamic dynamicMessage = new ExpandoObject();

            dynamicMessage.UserId = socketId;
            dynamicMessage.Message = message;

            _gameManager.Messages.Add(dynamicMessage);
            await InvokeClientMethodToAllAsync("pingMessage", socketId, message);
        }
    }
}
