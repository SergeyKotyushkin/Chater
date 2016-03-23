using Logic.Models;

namespace Logic.ElasticRepository.Contracts
{
    public interface IUserRepository
    {
        ElasticResult Add(string login, string password, string userName);

        ElasticResult Update(User user);

        ElasticResult Login(string login, string password);

        ElasticResult Get(string guid);

        ElasticResult GetByConnectionId(string connectionId);

        ElasticResult GetAll();

        ElasticResult GetAllOnline();

        ElasticResult GetByGuids(params string[] userGuids);
    }
}