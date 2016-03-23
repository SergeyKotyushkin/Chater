using Logic.Models;
using Nest;

namespace Logic.ElasticRepository.Contracts
{
    public interface IElasticRepository
    {
        string EsIndex { get; }

        ElasticResponse ExecuteSearchRequest<T>(SearchDescriptor<T> searchDescriptor) where T : class;

        ElasticResponse ExecuteCreateOrUpdateRequest<T>(T @object, IndexDescriptor<T> indexDescriptor) where T : class;

        ElasticResponse ExecuteMultiGetRequest(MultiGetDescriptor multiGetDescriptor);

        ElasticResponse ExecuteDeleteRequest<T>(DocumentPath<T> documentPath) where T : class;

        ElasticResult GetUserIfOnlyOneUserInElasticResponse<T>(ElasticResponse response) where T : class;

        ElasticResult GetUsersFromElasticResponse<T>(ElasticResponse response) where T : class;
    }
}