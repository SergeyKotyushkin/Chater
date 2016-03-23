using Logic.Models;

namespace Logic.ElasticRepository.Contracts
{
    public interface IChatRepository
    {
        ElasticResult Add(string name);

        ElasticResult Remove(string guid);

        ElasticResult Get(string guid);

        ElasticResult GetByGuds(params string[] chatGuids);
    }
}