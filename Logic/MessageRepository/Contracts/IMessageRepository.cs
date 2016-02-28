using Logic.Models;

namespace Logic.MessageRepository.Contracts
{
    public interface IMessageRepository
    {
        Message Add(string chatGuid, string userGuid, string text);

        Message[] GetByChatId(string guid);
    }
}