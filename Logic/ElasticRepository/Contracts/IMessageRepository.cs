using Logic.Models;

namespace Logic.ElasticRepository.Contracts
{
    public interface IMessageRepository
    {
        ElasticResult Add(string chatGuid, string userGuid, string text);

        ElasticResult GetByChatId(string guid);
    }
}