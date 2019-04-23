﻿using System;
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
        public GameHandler (WebSocketConnectionManager webSocketConnectionManager, GameManager gameManager): base(webSocketConnectionManager)
        {
            _gameManager = gameManager;
        }

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

        public async Task CreateRoom(string socketId, string username)
        {
            Classes.Room Room = new Classes.Room();

            Room.GenerateCode();
            Room.RoomOwnerId = socketId;
            Room.RoomOwner = username;
            Room.Users.Add(new Classes.User { SocketId = socketId, Username = username });

            _gameManager.Rooms.Add(Room);

            await InvokeClientMethodToAllAsync("returnRoomCode", socketId, Room.RoomCode);
        }

        public async Task JoinRoom(string socketId, string username, string roomCode)
        {
            foreach(Classes.Room room in _gameManager.Rooms)
            {
                if (room.RoomCode == roomCode)
                {
                    room.Users.Add(new Classes.User { SocketId = socketId, Username = username });
                    await InvokeClientMethodToAllAsync("joinRoom", socketId, roomCode);
                }
            }
        }

        public async Task LeaveRoom(string socketId, string roomCode)
        {
            foreach (Classes.Room room in _gameManager.Rooms)
            {
                if (room.RoomCode == roomCode)
                {
                    foreach (Classes.User user in room.Users)
                    {
                        if (user.SocketId == socketId)
                        {
                            room.Users.Remove(user);

                            if (room.Users.Count == 0)
                            {
                                _gameManager.Rooms.Remove(room);
                            }

                            await InvokeClientMethodToAllAsync("leaveRoom", socketId);
                        }
                    }
                }
            }
        }

        public async Task RetrieveRoomCount()
        {
            await InvokeClientMethodToAllAsync("retrieveRoomCount", _gameManager.Rooms.Count);
        }
    }
}
