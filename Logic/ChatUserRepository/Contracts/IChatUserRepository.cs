using Logic.Models;

namespace Logic.ChatUserRepository.Contracts
{
    public interface IChatUserRepository
    {
        bool CheckConnection();

        ChatUser Add(string chatGuid, string userGuid);

        bool Remove(string guid);

        ChatUser[] GetAllByUserGuid(string userGuid);

        ChatUser[] GetAllByChatGuid(string chatGuid);
    }
}