using System.Linq;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Logic.StructureMap;
using Nest;

namespace Logic.ElasticRepository
{
    public class ChatRepository : IChatRepository
    {
        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();
        private readonly IEntityRepository _entityRepository = StructureMapFactory.Resolve<IEntityRepository>();
        
        private const string EsType = "chat";


        // Adding new Chat.
        public ElasticResult Add(string name)
        {
            var chat = new Chat(name);
            var response = CheckChat(chat);

            return !response.Success ? response : _entityRepository.Add(EsType, chat);
        }

        // Removing the Chat
        public ElasticResult Remove(string guid)
        {
            var documentPath = new DocumentPath<Chat>(guid).Index(_elasticRepository.EsIndex).Type(EsType);
            var response = _elasticRepository.ExecuteDeleteRequest(documentPath);

            return response.Success
                ? new ElasticResult(true, "Success Removed", true)
                : new ElasticResult(false, response.Message);
        }

        // Getting Chat by Guid
        public ElasticResult Get(string guid)
        {
            return _entityRepository.Get<Chat>(EsType, guid);
        }

        // Getting Chats by Guids
        public ElasticResult GetByGuds(params string[] chatGuids)
        {
            return _entityRepository.GetByGuids<Chat>(EsType, chatGuids);
        }

        // Check Chat Is Unique
        private ElasticResult CheckChat(Chat chat)
        {
            var searchDescriptor = new SearchDescriptor<Chat>().Query(
                q => q.Term(t => t.Field(f => f.Guid).Value(chat.Guid))).Index(_elasticRepository.EsIndex).Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            // If request well executed. And user is unique.
            if (response.Success && !((ISearchResponse<Chat>)response.Response).Hits.Any())
                return new ElasticResult(true, true);

            return response.Success
                ? new ElasticResult(false, "UserName or Login are not unique!")
                : new ElasticResult(false, response.Message);
        }
    }
}