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
            var user = _userRepository.GetByConnectionId(id);
            if (user == null)
                return JsonConvert.SerializeObject(new Chat[] { });

            var chatUsers = _chatUserRepository.GetAllByUserGuid(user.Guid);
            var chats = _chatRepository.GetByGuds(chatUsers.Select(c => c.ChatGuid).ToArray());
            return JsonConvert.SerializeObject(chats);
        }
    }
}
