using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatBot.Models
{
    public class LoginData
    {
        public string UserId { get; set; }
    }

    public class UserData
    {
        public string UserId { get; set; }
        public string RoomId { get; set; }
    }
}