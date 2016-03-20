using Nest;

namespace Logic.Models
{
    [ElasticsearchType]
    public class ChatUser
    {
        public ChatUser(string chatGuid, string userGuid)
        {
            Guid = System.Guid.NewGuid().ToString();
            ChatGuid = chatGuid;
            UserGuid = userGuid;
        }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Guid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string ChatGuid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string UserGuid { get; set; }
    }
}