using System.Linq;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Logic.StructureMap;
using Nest;

namespace Logic.ElasticRepository
{
    public class ChatUserRepository : IChatUserRepository
    {
        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();
        private readonly IEntityRepository _entityRepository = StructureMapFactory.Resolve<IEntityRepository>();

        private const string EsType = "chatuser";

        // Creating new ChatUser
        public ElasticResult Add(string chatGuid, string userGuid)
        {
            var chatUser = new ChatUser(chatGuid, userGuid);
            var response = CheckChatUser(chatUser);

            return !response.Success ? response : _entityRepository.Add(EsType, chatUser);
        }

        // Deleting ChatUser by Guid
        public ElasticResult Remove(string guid)
        {
            return _entityRepository.Remove<ChatUser>(EsType, guid);
        }

        // Getting ChatUsers by User Guid
        public ElasticResult GetAllByUserGuid(string userGuid)
        {
            var searchDescriptor = new SearchDescriptor<ChatUser>().Query(
                q => q.Term(t => t.Field(f => f.UserGuid).Value(userGuid))).Index(_elasticRepository.EsIndex).Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntitiesFromElasticResponse<ChatUser>(response);
        }

        // Getting ChatUsers by ChatGuid
        public ElasticResult GetByChatGuid(string chatGuid)
        {
            var searchDescriptor = new SearchDescriptor<ChatUser>().Query(
                q => q.Term(t => t.Field(f => f.ChatGuid).Value(chatGuid))).Index(_elasticRepository.EsIndex).Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntitiesFromElasticResponse<ChatUser>(response);
        }

        // Check Is ChatUser Unique
        private ElasticResult CheckChatUser(ChatUser chatUser)
        {
            var searchDescriptor = new SearchDescriptor<ChatUser>().Query(
                q =>
                    q.Bool(
                        b =>
                            b.Must(
                                m => m.Term(t => t.Field(f => f.ChatGuid).Value(chatUser.ChatGuid)),
                                m => m.Term(t => t.Field(f => f.UserGuid).Value(chatUser.UserGuid)))))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            // If request well executed. And user is unique.
            if (response.Success && !((ISearchResponse<ChatUser>)response.Response).Hits.Any())
                return new ElasticResult(true, true);

            return response.Success
                ? new ElasticResult(false, "User in this chat already!")
                : new ElasticResult(false, response.Message);
        }
    }
}