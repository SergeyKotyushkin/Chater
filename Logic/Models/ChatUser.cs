namespace Logic.Models
{
    public class ChatUser
    {
        public ChatUser(string chatGuid, string userGuid)
        {
            Guid = System.Guid.NewGuid().ToString();
            ChatGuid = chatGuid;
            UserGuid = userGuid;
        }

        public string Guid { get; set; }

        public string ChatGuid { get; set; }

        public string UserGuid { get; set; }
    }
}