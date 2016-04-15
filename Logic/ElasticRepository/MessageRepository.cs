using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Logic.StructureMap;
using Nest;

namespace Logic.ElasticRepository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();
        private readonly IEntityRepository _entityRepository = StructureMapFactory.Resolve<IEntityRepository>();

        private const string EsType = "message";

        public ElasticResult Add(string chatGuid, string userGuid, string text)
        {
            var message = new Message(chatGuid, userGuid, text);

            return _entityRepository.Add(EsType, message);
        }

        public ElasticResult GetByChatId(string guid)
        {
            var searchDescriptor = new SearchDescriptor<Message>().Query(
                q => q.Term(t => t.Field(f => f.ChatGuid).Value(guid))).Index(_elasticRepository.EsIndex).Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntitiesFromElasticResponse<Message>(response);
        }
    }
}