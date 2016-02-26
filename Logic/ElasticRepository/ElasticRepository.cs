using System;
using System.Collections.Generic;
using System.Linq;
using Logic.ElasticRepository.Contracts;
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

        public ElasticClient GetElasticClient()
        {
            var node = new Uri(EsUri);

            var settings = new ConnectionSettings(node);
            //settings.PingTimeout(TimeSpan.FromSeconds(3));
            settings.RequestTimeout(TimeSpan.FromSeconds(3));

            return new ElasticClient(settings);
        }
    }
}