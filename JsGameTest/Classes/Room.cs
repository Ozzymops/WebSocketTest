using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace JsGameTest.Classes
{
    public class Room
    {
        public string RoomCode { get; set; }
        public string RoomOwnerId { get; set; }
        public string RoomOwner { get; set; }
        public enum State { Idle, Waiting, InProgress, Finished, Dead };
        public State RoomState { get; set; }
        public List<Classes.User> Users { get; set; } = new List<Classes.User>();
        public List<dynamic> Messages { get; set; } = new List<dynamic>();
        public int IdleTime = 10;

        public Room()
        {
            GenerateCode();

            // Timer ticks every second
            Timer timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(IdleTimer);
            timer.Start();
        }

        public void GenerateCode()
        {
            string code = "";

            // Six digit code
            for (int i = 0; i < 6; i++)
            {
                Random rng = new Random();
                if (rng.Next(0, 2) == 0)
                {
                    // Letter
                    if (rng.Next(0, 2) == 0)
                    {
                        // Upper - dec: 65 to 90
                        code += Char.ConvertFromUtf32(rng.Next(65, 91));
                    }
                    else
                    {
                        // Lower - dec: 97 to 122
                        code += Char.ConvertFromUtf32(rng.Next(97, 123));
                    }
                }
                else
                {
                    // Number
                    code += rng.Next(0, 10).ToString();
                }
            }

            RoomCode = code;
            RoomState = State.Waiting;
        }

        public void IdleTimer(object sender, ElapsedEventArgs e)
        {
            // Tick timer - reset in every function call from handler
            if (IdleTime > 0)
            {
                IdleTime -= 1;
            }
            
            // Go to idle mode after two minutes of inactivity
            if (IdleTime <= 0 && RoomState != State.Idle && RoomState != State.Dead)
            {
                IdleTime = 10;
                RoomState = State.Idle;
            }

            // Die when idle for two minutes
            if (IdleTime <= 0 && RoomState == State.Idle)
            {
                RoomState = State.Dead;               
            }
        }
    }
}
