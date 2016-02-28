using Logic.Models;

namespace Logic.ChatRepository.Contracts
{
    public interface IChatRepository
    {
        Chat Add(string name);

        bool Remove(string guid);

        Chat Get(string guid);

        Chat[] GetByIds(params string[] chatGuids);
    }
}