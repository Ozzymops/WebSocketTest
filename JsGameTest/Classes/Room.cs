using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace JsGameTest.Classes
{
    public class Room
    {
        // Static
        public State RoomState { get; set; }
        public int MaxIdleStrikes = 3;
        public int MaxProgressStrikes = 20;
        // Dynamic
        public int CurrentStrikes;
        public string RoomCode { get; set; }
        public string RoomOwnerId { get; set; }
        public string RoomOwner { get; set; }
        public enum State { Waiting, InProgress, Finished, Dead };
        public List<Classes.User> Users { get; set; } = new List<Classes.User>();
        public List<dynamic> Messages { get; set; } = new List<dynamic>();
        

        // Configuration

        public Timer timer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds); // Tick every sixty seconds

        /// <summary>
        /// Constructor: generate random code and set timer.
        /// </summary>
        public Room()
        {
            GenerateCode();

            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(IdleTimer);
            timer.Start();
        }

        /// <summary>
        /// Generate code - random six digit code consisting of upper- and lowercase letters and numbers.
        /// </summary>
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

        /// <summary>
        /// Idle timer - check if the room is still being actively used.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IdleTimer(object sender, ElapsedEventArgs e)
        {
            // Tick timer - reset in every function call from handler
            if (CurrentStrikes > 0)
            {
                CurrentStrikes -= 1;
            }

            // Die after strikes are up
            if (CurrentStrikes <= 0 && RoomState != State.Dead)
            {
                RoomState = State.Dead;
                timer.Stop();
            }
        }

        /// <summary>
        /// Reset the idle timer to the maximum allowed strikes.
        /// </summary>
        public void ResetTimer()
        {
            if (RoomState == State.Waiting)
            {
                CurrentStrikes = MaxIdleStrikes;
            }
            else if (RoomState == State.InProgress)
            {
                CurrentStrikes = MaxProgressStrikes;
            }
        }

        public void RandomizeGroups()
        {

        }
    }
}
