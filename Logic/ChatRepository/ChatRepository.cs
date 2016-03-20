using System.Linq;
using Logic.ChatRepository.Contracts;
using Logic.Models;
using Nest;

namespace Logic.ChatRepository
{
    public class ChatRepository : ElasticRepository.ElasticRepository, IChatRepository
    {
        private const string EsType = "chat";

        public static string EsIndex
        {
            get { return "database"; }
        }

        public Chat Add(string name)
        {
            try
            {
                var chat = new Chat(name);
                if (!CheckChat(chat))
                    return null;

                CreateIndex<Chat>(EsIndex);

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

        public Chat Get(string guid)
        {
            try
            {
                var client = GetElasticClient();
                var hits =
                    client.Search<Chat>(
                        s => s.Query(q => q.Term(t => t.Field(f => f.Guid).Value(guid))).Index(EsIndex).Type(EsType))
                        .Hits;

                var result = hits as IHit<Chat>[] ?? hits.ToArray();
                return result.Count() == 1? result[0].Source: null;
            } 
            catch
            {
                return null;
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
                    client.Search<Chat>(
                        s =>
                            s.Query(
                                q =>
                                    q.Bool(
                                        b =>
                                            b.Should(
                                                sh => sh.Term(t => t.Field(f => f.Guid).Value(chat.Guid)),
                                                sh => sh.Term(t => t.Field(f => f.Name).Value(chat.Name))))))
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