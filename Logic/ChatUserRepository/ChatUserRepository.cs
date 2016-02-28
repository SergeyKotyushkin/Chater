using System.Linq;
using Logic.ChatUserRepository.Contracts;
using Logic.Models;

namespace Logic.ChatUserRepository
{
    public class ChatUserRepository : ElasticRepository.ElasticRepository, IChatUserRepository
    {
        private const string EsIndex = "database";
        private const string EsType = "chatuser";

        public ChatUser Add(string chatGuid, string userGuid)
        {
            try
            {
                var chatUser = new ChatUser(chatGuid, userGuid);
                if (!CheckChatUser(chatUser))
                    return null;

                var client = GetElasticClient();
                client.Index(chatUser, i => i.Index(EsIndex).Type(EsType).Id(chatUser.Guid));
                return chatUser;
            }
            catch
            {
                return null;
            }
        }

        public bool Remove(string guid)
        {
            try
            {
                var client = GetElasticClient();
                client.Delete<ChatUser>(guid, d => d.Index(EsIndex).Type(EsType));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ChatUser[] GetAllForUser(string userGuid)
        {
            try
            {
                var client = GetElasticClient();
                var hits =
                    client.Search<ChatUser>(s => s
                        .Query(q => q.Match(m => m.Field("userGuid").Query(userGuid)))
                        .Index(EsIndex).Type(EsType)).Hits;

                return hits.Select(hit => hit.Source).ToArray();
            }
            catch
            {
                return new ChatUser[]{};
            }
        }

        public ChatUser[] GetAllByChatGuid(string chatGuid)
        {
            try
            {
                var client = GetElasticClient();
                var hits =
                    client.Search<ChatUser>(s => s
                        .Query(q => q.Match(m => m.Field("chatGuid").Query(chatGuid)))
                        .Index(EsIndex).Type(EsType)).Hits;

                return hits.Select(hit => hit.Source).ToArray();
            }
            catch
            {
                return new ChatUser[] { };
            }
        }


        private bool CheckChatUser(ChatUser chatUser)
        {
            try
            {
                var client = GetElasticClient();
                var hitsCount =
                    client.Search<ChatUser>(s => s
                        .Query(q => q.Match(m => m.Field("chatGuid").Query(chatUser.ChatGuid)) && 
                                    q.Match(m => m.Field("userGuid").Query(chatUser.UserGuid)))
                        .Index(EsIndex).Type(EsType)).Hits.Count();

                return hitsCount == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}