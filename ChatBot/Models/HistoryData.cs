using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatBot.Models
{
    public class HistoryData
    {
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class ChatDetail : HistoryData
    {
        public string ChatContent { get; set; }
        public string ChatOutContent { get; set; }
        public string ChatId { get; set; }
    }
}