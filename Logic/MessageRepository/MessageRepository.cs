using System.Linq;
using Logic.MessageRepository.Contracts;
using Logic.Models;

namespace Logic.MessageRepository
{
    public class MessageRepository : ElasticRepository.ElasticRepository, IMessageRepository
    {
        private const string EsIndex = "database";
        private const string EsType = "message";

        public Message Add(string chatGuid, string userGuid, string text)
        {
            try
            {
                var message = new Message(chatGuid, userGuid, text);

                var client = GetElasticClient();
                client.Index(message, i => i.Index(EsIndex).Type(EsType).Id(message.Guid));
                return message;
            }
            catch
            {
                return null;
            }
        }

        public Message[] GetByChatId(string guid)
        {
            try
            {
                var client = GetElasticClient();
                var hits =
                    client.Search<Message>(
                        s => s.Query(q => q.Match(m => m.Field("chatGuid").Query(guid)))
                              .Index(EsIndex).Type(EsType)).Hits;

                return hits.Select(m => m.Source).ToArray();
            }
            catch
            {
                return null;
            }
        }
    }
}