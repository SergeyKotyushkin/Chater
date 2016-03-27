using Logic.Models;

namespace Logic.ElasticRepository.Contracts
{
    public interface IChatRepository
    {
        ElasticResult Add(string name);

        ElasticResult Update(Chat chat);

        ElasticResult Remove(string guid);

        ElasticResult Get(string guid);

        ElasticResult GetByGuids(params string[] chatGuids);
    }
}