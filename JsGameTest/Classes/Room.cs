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
        public int IdleStrikes = 3;

        public Timer timer = new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds); // Tick every sixty seconds

        public Room()
        {
            GenerateCode();

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
            if (RoomState != State.InProgress)
            {
                // Tick timer - reset in every function call from handler
                if (IdleStrikes > 0)
                {
                    IdleStrikes -= 1;
                }

                // Die after strikes are up
                if (IdleStrikes <= 0 && RoomState != State.Dead)
                {
                    RoomState = State.Dead;
                    timer.Stop();
                }
            }
        }

        public void ResetTimer()
        {
            IdleStrikes = 3;
        }
    }
}
