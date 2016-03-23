using System.Linq;
using Logic.Contracts;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Logic.StructureMap;
using Nest;

namespace Logic.ElasticRepository
{
    public class EntityRepository : IEntityRepository
    {
        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();

        public ElasticResult Add<T>(string esType, T @object) where T : class, IGuidedEntity
        {
            var indexDescriptor = new IndexDescriptor<T>(_elasticRepository.EsIndex, esType).Id(@object.Guid);
            var response = _elasticRepository.ExecuteCreateOrUpdateRequest(@object, indexDescriptor);

            return response.Success
                ? new ElasticResult(true, "Success Added", @object)
                : new ElasticResult(false, response.Message);
        }

        public ElasticResult Get<T>(string esType, string guid) where T : class, IGuidedEntity
        {
            var searchDescriptor = new SearchDescriptor<T>().Query(
                q => q.Term(t => t.Field(f => f.Guid).Value(guid))).Index(_elasticRepository.EsIndex).Type(esType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _elasticRepository.GetUserIfOnlyOneUserInElasticResponse<T>(response);
        }

        public ElasticResult GetByGuids<T>(string esType, params string[] guids) where T : class
        {
            var multiGetDescriptor = new MultiGetDescriptor().GetMany<T>(guids).Index(_elasticRepository.EsIndex).Type(esType);

            var response = _elasticRepository.ExecuteMultiGetRequest(multiGetDescriptor);

            if (!response.Success)
                return new ElasticResult(false, response.Message);

            var multiGetResponse = (IMultiGetResponse)response.Response;
            var hits = multiGetResponse.GetMany<T>(guids);

            return new ElasticResult(true, hits.Select(hit => hit.Source).Where(hit => hit != null).ToArray());
        }

        public ElasticResult GetAll<T>(string esType) where T : class
        {
            var searchDescriptor = new SearchDescriptor<T>().AllIndices().Index(_elasticRepository.EsIndex).Type(esType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _elasticRepository.GetUsersFromElasticResponse<T>(response);
        }

        public ElasticResult Remove<T>(string esType, string guid) where T : class
        {
            var documentPath = new DocumentPath<T>(guid).Index(_elasticRepository.EsIndex).Type(esType);
            var response = _elasticRepository.ExecuteDeleteRequest(documentPath);

            return response.Success
                ? new ElasticResult(true, "Success Removed", true)
                : new ElasticResult(false, response.Message);
        }
    }
}