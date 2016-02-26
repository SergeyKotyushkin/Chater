using Logic.Models;

namespace Logic.UserRepository.Contracts
{
    public interface IUserRepository
    {
        User Add(string connectionId, string login, string password, string userName, bool isOnline);

        User Update(User user);

        User Login(string login, string password);

        User Get(string guid);

        User GetByConnectionId(string connectionId);

        User[] GetAll();

        User[] GetAllOnline();
    }
}