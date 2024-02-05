using ChatUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatBot.Models
{
    public class UserMessage : Message
    {
        public string userId { get; set; }
        public string roomId { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }

        public Message() { }

        public Message(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
}