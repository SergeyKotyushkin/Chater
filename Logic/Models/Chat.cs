using Logic.Contracts;
using Nest;
using Newtonsoft.Json;

namespace Logic.Models
{
    [ElasticsearchType]
    public class Chat : IGuidedEntity
    {
        [JsonConstructor]
        public Chat(string name)
        {
            Guid = System.Guid.NewGuid().ToString();
            Name = name;
        }

        public Chat(string guid, string name)
        {
            Guid = guid;
            Name = name;
        }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Guid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Name { get; set; }
    }
}