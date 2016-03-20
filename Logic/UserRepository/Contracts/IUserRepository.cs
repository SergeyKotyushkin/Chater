using Logic.Models;

namespace Logic.UserRepository.Contracts
{
    public interface IUserRepository
    {
        User Add(string login, string password, string userName);

        User Update(User user);

        User Login(string login, string password);

        User Get(string guid);

        User GetByConnectionId(string connectionId);

        User[] GetAll();

        User[] GetAllOnline();

        User[] GetByGuids(params string[] userGuids);
    }
}