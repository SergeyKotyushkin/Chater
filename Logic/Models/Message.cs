using System;
using Logic.Contracts;

namespace Logic.Models
{
    public class Message : IGuidedEntity
    {
        public Message(string chatGuid, string userGuid, string text)
        {
            Guid = System.Guid.NewGuid().ToString();
            ChatGuid = chatGuid;
            UserGuid = userGuid;
            SendTime = DateTime.Now;
            Text = text;
        }

        public string Guid { get; set; }

        public string ChatGuid { get; set; }

        public string UserGuid { get; set; }

        public DateTime SendTime { get; set; }

        public string Text { get; set; }
    }
}