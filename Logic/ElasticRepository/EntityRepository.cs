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

        // Creating Entity for Type
        public ElasticResult Add<T>(string esType, T @object) where T : class, IGuidedEntity
        {
            var indexDescriptor = new IndexDescriptor<T>(_elasticRepository.EsIndex, esType).Id(@object.Guid);
            var response = _elasticRepository.ExecuteCreateOrUpdateRequest(@object, indexDescriptor);

            return response.Success
                ? new ElasticResult(true, "Success Added", @object)
                : new ElasticResult(false, response.Message);
        }

        // Getting Entity for Type by Guids
        public ElasticResult Get<T>(string esType, string guid) where T : class, IGuidedEntity
        {
            var searchDescriptor = new SearchDescriptor<T>().Query(
                q => q.Term(t => t.Field(f => f.Guid).Value(guid))).Index(_elasticRepository.EsIndex).Type(esType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return GetEntityIfOnlyOneEntityInElasticResponse<T>(response);
        }

        // Getting all Entities for Type by Guids
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

        // Getting all Entities for Type
        public ElasticResult GetAll<T>(string esType) where T : class
        {
            var searchDescriptor = new SearchDescriptor<T>().AllIndices().Index(_elasticRepository.EsIndex).Type(esType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return GetEntitiesFromElasticResponse<T>(response);
        }

        // Removing Entity for Type by Guid
        public ElasticResult Remove<T>(string esType, string guid) where T : class
        {
            var documentPath = new DocumentPath<T>(guid).Index(_elasticRepository.EsIndex).Type(esType);
            var response = _elasticRepository.ExecuteDeleteRequest(documentPath);

            return response.Success
                ? new ElasticResult(true, "Success Removed", true)
                : new ElasticResult(false, response.Message);
        }
        
        // Taking from the Elastic Response an Entity if this is only one there
        public ElasticResult GetEntityIfOnlyOneEntityInElasticResponse<T>(ElasticResponse response) where T : class
        {
            // If request bad executed.
            if (!response.Success)
                return new ElasticResult(false, response.Message);

            var hits = ((ISearchResponse<T>)response.Response).Hits;
            var hitsArray = hits as IHit<T>[] ?? hits.ToArray();

            // If found other than 1 user
            if (response.Success && hitsArray.Count() != 1)
                return new ElasticResult(true, response.Message);

            return new ElasticResult(true, hitsArray.ElementAt(0));
        }

        // Taking all Entities from the Elastic Response
        public ElasticResult GetEntitiesFromElasticResponse<T>(ElasticResponse response) where T : class
        {
            // If request bad executed.
            if (!response.Success)
                return new ElasticResult(false, response.Message);

            return new ElasticResult(true,
                ((ISearchResponse<T>)response.Response).Hits.Select(h => h.Source).Where(s => s != null).ToArray());
        }
    }
}