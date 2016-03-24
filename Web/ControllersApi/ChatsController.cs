using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Newtonsoft.Json;

namespace Web.ControllersApi
{
    public class ChatsController : ApiController
    {
        private readonly IChatUserRepository _chatUserRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        
        public ChatsController(IChatUserRepository chatUserRepository, IChatRepository chatRepository,
            IUserRepository userRepository)
        {
            _chatUserRepository = chatUserRepository;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }

        // GET api/chats/5
        public string Get(string id)
        {
            var elasticResult = _userRepository.GetByConnectionId(id);
            if (!elasticResult.Success)
                return JsonConvert.SerializeObject(new Chat[] {});

            var user = (User) elasticResult.Value;

            elasticResult = _chatUserRepository.GetAllByUserGuid(user.Guid);
            if (!elasticResult.Success)
                return JsonConvert.SerializeObject(new Chat[] {});

            var chatUsers = (ChatUser[]) elasticResult.Value;

            elasticResult = _chatRepository.GetByGuids(chatUsers.Select(c => c.ChatGuid).ToArray());
            if (!elasticResult.Success)
                return JsonConvert.SerializeObject(new Chat[] {});

            var chats = (Chat[]) elasticResult.Value;
            return JsonConvert.SerializeObject(chats);
        }
    }
}
