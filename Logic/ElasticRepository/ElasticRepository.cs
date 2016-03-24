using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Contracts;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Nest;

namespace Logic.ElasticRepository
{
    public class ElasticRepository : IElasticRepository
    {
        private const string EsUri = "http://localhost:9200";
        private const string EsIndexName = "database";


        /* IElasticRepository members */
        /* Properties */
        public string EsIndex
        {
            get { return EsIndexName; }
        }

        /* Methods */
        // Searching entity at the Elasticsearch database
        public ElasticResponse ExecuteSearchRequest<T>(SearchDescriptor<T> searchDescriptor) where T : class
        {
            try
            {
                var client = GetElasticClient();
                var response = client.Search<T>(s => searchDescriptor);

                if (response.TimedOut)
                    return new ElasticResponse(false, "The Request's Timeout was exceeded");

                return !response.ApiCall.Success
                    ? new ElasticResponse(false,
                        "Request ended with error: " + response.ApiCall.OriginalException.Message)
                    : new ElasticResponse(true, response);
            }
            catch
            {
                return new ElasticResponse(false, "Server error.");
            }
        }

        // Creting or Updating entity at the Elasticsearch database
        public ElasticResponse ExecuteCreateOrUpdateRequest<T>(T @object, string esType) where T : class, IGuidedEntity
        {
            try
            {
                var client = GetElasticClient();
                var response = client.Index(@object, i => i.Index(EsIndex).Type(esType).Id(@object.Guid));

                return response.ApiCall.Success
                    ? new ElasticResponse(true, true)
                    : new ElasticResponse(false,
                        "Request ended with error. " + response.ApiCall.OriginalException.Message);
            }
            catch
            {
                return new ElasticResponse(false, "Server error.");
            }
        }

        // MultiGet Entities by indices from the Elasticsearch database
        public ElasticResponse ExecuteMultiGetRequest(MultiGetDescriptor multiGetDescriptor)
        {
            try
            {
                var client = GetElasticClient();
                var response = client.MultiGet(m => multiGetDescriptor);

                return response.ApiCall.Success
                    ? new ElasticResponse(true, response)
                    : new ElasticResponse(false,
                        "Request ended with error. " + response.ApiCall.OriginalException.Message);
            }
            catch
            {
                return new ElasticResponse(false, "Server error.");
            }
        }
        
        // Deleting Entity by indices from the Elasticsearch database
        public ElasticResponse ExecuteDeleteRequest<T>(DocumentPath<T> documentPath) where T : class
        {
            try
            {
                var client = GetElasticClient();
                var response = client.Delete(documentPath);

                return response.ApiCall.Success
                    ? new ElasticResponse(true, true)
                    : new ElasticResponse(false, "Request ended with error. " + response.ApiCall.OriginalException.Message);
            }
            catch
            {
                return new ElasticResponse(false, "Server error.");
            }
        }


        /* Public Static Methods */
        // Creating Index
        public static ElasticResult ElasticSearchCreateIndices()
        {
            try
            {
                var client = GetElasticClient();

                var isIndexExist = client.IndexExists(Indices.All, i => i.Index(EsIndexName)).Exists;
                if (isIndexExist)
                    return new ElasticResult(true, true);

                var response = client.CreateIndex(EsIndexName,
                    i =>
                        i.Mappings(
                            m => m
                                .Map<User>(map => map.AutoMap())
                                .Map<Chat>(map => map.AutoMap())
                                .Map<ChatUser>(map => map.AutoMap())));

                return new ElasticResult(true, true);
            }
            catch
            {
                return new ElasticResult(false, "Server error.");
            }
        }


        /* Private Static Methods */
        // Getting Elasting Client
        private static ElasticClient GetElasticClient()
        {
            var node = new Uri(EsUri);

            var settings = new ConnectionSettings(node);
            settings.RequestTimeout(TimeSpan.FromSeconds(3));

            return new ElasticClient(settings);
        }
    }
}