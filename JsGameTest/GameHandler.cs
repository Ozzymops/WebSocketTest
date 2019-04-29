using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using WebSocketManager;

namespace JsGameTest
{
    public class GameHandler : WebSocketHandler
    {
        private readonly GameManager _gameManager;
        public GameHandler (WebSocketConnectionManager webSocketConnectionManager, GameManager gameManager): base(webSocketConnectionManager)
        {
            _gameManager = gameManager;

            // Timer ticks every five seconds.
            Timer timer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(CheckRoomStates);
            timer.Start();
        }

        /// <summary>
        /// Send a message to everyone else in the room.
        /// </summary>
        /// <param name="socketId">Sender ID</param>
        /// <param name="username">Sender username</param>
        /// <param name="message">Message</param>
        /// <param name="roomCode">Room</param>
        /// <returns></returns>
        public async Task SendMessage(string socketId, string username, string message, string roomCode)
        {
            dynamic dynamicMessage = new ExpandoObject();

            dynamicMessage.UserId = socketId;
            dynamicMessage.Username = username;
            dynamicMessage.Message = message;
            dynamicMessage.RoomCode = roomCode;

            foreach(Classes.Room room in _gameManager.Rooms)
            {
                if (room.RoomCode == roomCode)
                {
                    foreach (Classes.User user in room.Users)
                    {
                        if (user.SocketId == socketId)
                        {
                            room.Messages.Add(dynamicMessage);
                            await InvokeClientMethodToAllAsync("pingMessage", socketId, username, message, roomCode);
                        }
                    }
                }
            }            
        }

        /// <summary>
        /// Send a message to everyone else in the room without a sender.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="roomCode">Room</param>
        /// <returns></returns>
        public async Task ServerMessage(string message, string roomCode)
        {
            await InvokeClientMethodToAllAsync("serverMessage", message, roomCode);
        }

        /// <summary>
        /// Open a room instance.
        /// </summary>
        /// <param name="socketId">Owner ID</param>
        /// <param name="username">Owner username</param>
        /// <returns></returns>
        public async Task CreateRoom(string socketId, string username)
        {
            Classes.Room Room = new Classes.Room();

            Room.RoomOwnerId = socketId;
            Room.RoomOwner = username;
            Room.Users.Add(new Classes.User { SocketId = socketId, Username = username });

            _gameManager.Rooms.Add(Room);

            await InvokeClientMethodToAllAsync("returnRoomCode", socketId, Room.RoomCode);
        }

        /// <summary>
        /// Join a room instance.
        /// </summary>
        /// <param name="socketId">Client ID</param>
        /// <param name="username">Client username</param>
        /// <param name="roomCode">Room</param>
        /// <returns></returns>
        public async Task JoinRoom(string socketId, string username, string roomCode)
        {
            foreach(Classes.Room room in _gameManager.Rooms)
            {
                if (room.RoomCode == roomCode)
                {
                    if (room.RoomState == Classes.Room.State.Waiting || room.RoomState == Classes.Room.State.Idle)
                    {
                        room.Users.Add(new Classes.User { SocketId = socketId, Username = username });
                        await InvokeClientMethodToAllAsync("joinRoom", socketId, roomCode);
                    }
                    else
                    {
                        string message = "Failed to join room - game is already in progress or finished.";
                        await InvokeClientMethodToAllAsync("setStateMessage", socketId, message);
                    }
                }
            }
        }

        /// <summary>
        /// Leave a room instance.
        /// </summary>
        /// <param name="socketId">Client ID</param>
        /// <param name="roomCode">Room</param>
        /// <returns></returns>
        public async Task LeaveRoom(string socketId, string roomCode)
        {
            foreach (Classes.Room room in _gameManager.Rooms)
            {
                if (room.RoomCode == roomCode)
                {
                    // If owner leaves...
                    if (socketId == room.RoomOwnerId)
                    {
                        foreach (Classes.User user in room.Users)
                        {
                            await InvokeClientMethodToAllAsync("leaveRoom", user.SocketId);

                            string message = "Room has died. Reason: owner left the room.";
                            await InvokeClientMethodToAllAsync("setStateMessage", user.SocketId, message);
                        }

                        _gameManager.Rooms.Remove(room);
                    }

                    // If regular user leaves
                    foreach (Classes.User user in room.Users)
                    {
                        if (user.SocketId == socketId)
                        {
                            room.Users.Remove(user);

                            await InvokeClientMethodToAllAsync("leaveRoom", socketId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete a room after it's been idle for too long.
        /// </summary>
        /// <param name="roomCode"></param>
        /// <returns></returns>
        public async Task KillRoom(string roomCode)
        {
            foreach (Classes.Room room in _gameManager.Rooms)
            {
                if (roomCode == room.RoomCode)
                {
                    foreach (Classes.User user in room.Users)
                    {                        
                        await InvokeClientMethodToAllAsync("leaveRoom", user.SocketId);

                        string message = "Room has died. Reason: idle for too long.";
                        await InvokeClientMethodToAllAsync("setStateMessage", user.SocketId, message);
                    }

                    _gameManager.Rooms.Remove(room);
                }
            }
        }

        /// <summary>
        /// Start a game with the current room.
        /// </summary>
        /// <param name="socketId">User ID</param>
        /// <param name="roomCode">Room</param>
        /// <returns></returns>
        public async Task StartGame(string socketId, string roomCode)
        {
            foreach (Classes.Room room in _gameManager.Rooms)
            {
                if (room.RoomCode == roomCode)
                {
                    room.RoomState = Classes.Room.State.InProgress;

                    await InvokeClientMethodToAllAsync("startGame", roomCode, socketId);
                }
            }
        }

        /// <summary>
        /// Retrieve the amount of rooms online.
        /// </summary>
        /// <returns></returns>
        public async Task RetrieveRoomCount()
        {
            await InvokeClientMethodToAllAsync("retrieveRoomCount", _gameManager.Rooms.Count);
        }

        /// <summary>
        /// Retrieve connected users inside of the current room.
        /// </summary>
        /// <param name="roomCode">Room</param>
        /// <returns></returns>
        public async Task RetrieveUserList(string roomCode)
        {
            string ownerId = "";
            List<string> UsernameList = new List<string>();

            foreach(Classes.Room room in _gameManager.Rooms)
            {
                if (room.RoomCode == roomCode)
                {
                    ownerId = room.RoomOwnerId;

                    foreach(Classes.User user in room.Users)
                    {
                        string tempString = user.Username + ":|!" + user.SocketId;
                        UsernameList.Add(tempString);
                    }
                }
            }

            await InvokeClientMethodToAllAsync("retrieveUserList", roomCode, ownerId, Newtonsoft.Json.JsonConvert.SerializeObject(UsernameList));
        }

        /// <summary>
        /// Check rooms for their states and handle accordingly.
        /// </summary>
        /// <returns></returns>
        public async void CheckRoomStates(object sender, ElapsedEventArgs e)
        {
            foreach (Classes.Room room in _gameManager.Rooms)
            {
                if (room.RoomState == Classes.Room.State.Dead)
                {
                    await KillRoom(room.RoomCode);
                }
            }
        }

        // DEBUG!!!
        public async Task CheckRoomState(string roomCode)
        {
            foreach (Classes.Room room in _gameManager.Rooms)
            {
                if (roomCode == room.RoomCode)
                {
                    string state = room.RoomState.ToString() + ": " + room.IdleTime.ToString() + "s.";
                    await InvokeClientMethodToAllAsync("checkRoomState", room.RoomCode, state);
                }
            }
        }
    }
}
