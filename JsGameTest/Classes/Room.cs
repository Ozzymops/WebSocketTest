using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsGameTest.Classes
{
    public class Room
    {
        public string RoomCode { get; set; }
        public string RoomOwnerId { get; set; }
        public string RoomOwner { get; set; }
        public List<Classes.User> Users { get; set; } = new List<Classes.User>();
        public List<dynamic> Messages { get; set; } = new List<dynamic>();

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
        }
    }
}
