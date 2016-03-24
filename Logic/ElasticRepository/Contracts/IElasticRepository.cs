using Logic.Contracts;
using Logic.Models;
using Nest;

namespace Logic.ElasticRepository.Contracts
{
    public interface IElasticRepository
    {
        string EsIndex { get; }

        ElasticResponse ExecuteSearchRequest<T>(SearchDescriptor<T> searchDescriptor) where T : class;

        ElasticResponse ExecuteCreateOrUpdateRequest<T>(T @object, string esType) where T : class, IGuidedEntity;

        ElasticResponse ExecuteMultiGetRequest(MultiGetDescriptor multiGetDescriptor);

        ElasticResponse ExecuteDeleteRequest<T>(DocumentPath<T> documentPath) where T : class;
    }
}