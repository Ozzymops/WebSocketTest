﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsGameTest.Classes
{
    public class Story
    {
        public int Id { get; set; }

        // OwnerId = User.Id
        public int OwnerId { get; set; }
        public bool IsRoot { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
    }
}
