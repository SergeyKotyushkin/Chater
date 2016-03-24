using Logic.Models;

namespace Logic.ElasticRepository.Contracts
{
    public interface IChatUserRepository
    {
        ElasticResult Add(string chatGuid, string userGuid);

        ElasticResult Remove(string guid);

        ElasticResult GetAllByUserGuid(string userGuid);

        ElasticResult GetUsersForChatByChatGuid(string chatGuid);
    }
}