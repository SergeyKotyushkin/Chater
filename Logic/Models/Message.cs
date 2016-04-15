using System;
using Logic.Contracts;
using Logic.ElasticRepository.Contracts;
using Logic.StructureMap;

namespace Logic.Models
{
    public class Message : IGuidedEntity
    {
        private readonly IUserRepository _userRepository = StructureMapFactory.Resolve<IUserRepository>();

        public Message(string chatGuid, string userGuid, string text)
        {
            Guid = System.Guid.NewGuid().ToString();
            ChatGuid = chatGuid;
            UserGuid = userGuid;
            UserName = GetUserName(userGuid);
            SendTime = DateTime.Now;
            Text = text;
        }

        public string Guid { get; set; }

        public string ChatGuid { get; set; }

        public string UserGuid { get; set; }

        public string UserName { get; set; }

        public DateTime SendTime { get; set; }

        public string Text { get; set; }

        
        private string GetUserName(string userGuid)
        {
            var elasticResult = _userRepository.Get(userGuid);
            return !elasticResult.Success ? null : ((User) elasticResult.Value).UserName;
        }
    }
}