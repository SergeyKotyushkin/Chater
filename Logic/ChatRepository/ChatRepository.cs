using System.Linq;
using Logic.ChatRepository.Contracts;
using Logic.Models;

namespace Logic.ChatRepository
{
    public class ChatRepository : ElasticRepository.ElasticRepository, IChatRepository
    {
        private const string EsIndex = "database";
        private const string EsType = "chat";

        public Chat Add(string name)
        {
            try
            {
                var chat = new Chat(name);
                if (!CheckChat(chat))
                    return null;

                var client = GetElasticClient();
                client.Index(chat, i => i.Index(EsIndex).Type(EsType).Id(chat.Guid));
                return chat;
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
                client.Delete<Chat>(guid, d => d.Index(EsIndex).Type(EsType));
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public Chat[] GetByIds(params string[] chatGuids)
        {
            try
            {
                var client = GetElasticClient();
                var result = client.MultiGet(m => m.GetMany<Chat>(chatGuids).Index(EsIndex).Type(EsType));
                var hits = result.GetMany<Chat>(chatGuids);

                return hits.Select(hit => hit.Source).Where(hit => hit != null).ToArray();
            }
            catch
            {
                return new Chat[] {};
            }
        }


        private bool CheckChat(Chat chat)
        {
            try
            {
                var client = GetElasticClient();
                var hitsCount =
                    client.Search<User>(s => s.Query(q => q.Match(m => m.Field("Guid").Query(chat.Guid))))
                        .Hits.Count();

                if (hitsCount != 0) return false;

                hitsCount =
                    client.Search<User>(s => s.Query(q => q.Match(m => m.Field("Name").Query(chat.Name))))
                        .Hits.Count();
                return hitsCount == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}