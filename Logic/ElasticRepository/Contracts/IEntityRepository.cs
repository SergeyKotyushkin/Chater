using Logic.Contracts;
using Logic.Models;

namespace Logic.ElasticRepository.Contracts
{
    public interface IEntityRepository
    {
        ElasticResult Add<T>(string esType, T @object) where T : class, IGuidedEntity;

        ElasticResult Get<T>(string esType, string guid) where T : class, IGuidedEntity;

        ElasticResult GetByGuids<T>(string esType, params string[] guids) where T : class;

        ElasticResult GetAll<T>(string esType) where T : class;

        ElasticResult Remove<T>(string esType, string guid) where T : class;

        ElasticResult GetEntityIfOnlyOneEntityInElasticResponse<T>(ElasticResponse response) where T : class;

        ElasticResult GetEntitiesFromElasticResponse<T>(ElasticResponse response) where T : class;
    }
}