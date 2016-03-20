using System.Collections.Generic;
using System.Linq;
using Nest;
using Newtonsoft.Json;
using Elasticsearch;

namespace Logic.Models
{
    [ElasticsearchType]
    public class User
    {
        // Constructors
        [JsonConstructor]
        public User(string guid, HashSet<string> connectionIds, string login, string password, string userName)
        {
            Guid = guid;
            ConnectionIds = connectionIds;
            Login = login;
            Password = password;
            UserName = userName;
        }

        public User(HashSet<string> connectionIds, string login, string password, string userName)
        {
            Guid = System.Guid.NewGuid().ToString();
            ConnectionIds = connectionIds;
            Login = login;
            Password = password;
            UserName = userName;
        }


        // Properties
        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Guid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public HashSet<string> ConnectionIds { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Login { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Password { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string UserName { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public bool IsOnline
        {
            get
            {
                return ConnectionIds != null && ConnectionIds.Any();
            }
        }


        // Methods
        public static IList<string> GetConnectionIds(IEnumerable<User> users)
        {
            var result = new HashSet<string>();
            foreach (
                var connectionId in users.Where(u => u.ConnectionIds != null).SelectMany(user => user.ConnectionIds))
                result.Add(connectionId);

            return result.ToArray();
        }
    }
}