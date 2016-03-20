using System;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Nest;

namespace Logic.ElasticRepository
{
    public class ElasticRepository : IElasticRepository
    {
        private const string EsUri = "http://localhost:9200";


        public bool CheckConnection()
        {
            try
            {
                GetElasticClient().Ping();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static ElasticClient GetElasticClient()
        {
            var node = new Uri(EsUri);

            var settings = new ConnectionSettings(node);
            settings.RequestTimeout(TimeSpan.FromSeconds(3));

            return new ElasticClient(settings);
        }

        public static void CreateIndex<T>(string esIndex) where T : class
        {
            var client = GetElasticClient();
            var isIndexExist = client.IndexExists(Indices.All, i => i.Index(esIndex)).Exists;
            if (isIndexExist) return;

            client.CreateIndex(esIndex, i => i.Mappings(m => m.Map<T>(map => map.AutoMap())));
        }

        public static void ElasticSearchCreateIndices()
        {
            var client = GetElasticClient();

            var isIndexExist =
                client.IndexExists(Indices.All, i => i.Index(UserRepository.UserRepository.EsIndex)).Exists;
            if (isIndexExist) return;

            client.CreateIndex(UserRepository.UserRepository.EsIndex,
                i =>
                    i.Mappings(
                        m => m
                            .Map<User>(map => map.AutoMap())
                            .Map<Chat>(map => map.AutoMap())
                            .Map<ChatUser>(map => map.AutoMap())));

        }
    }
}