using System.Linq;
using System.Web.Http;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Newtonsoft.Json;

namespace Web.ControllersApi
{
    public class MessagesController : ApiController
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public MessagesController(IChatRepository chatRepository, IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        // GET api/messages/5
        public string Get(string id)
        {
            var elasticResult = _chatRepository.Get(id);
            if (!elasticResult.Success)
                return null;


            elasticResult = _messageRepository.GetByChatId(id);
            if (!elasticResult.Success)
                return null;

            var messages = ((Message[]) elasticResult.Value).OrderBy(m => m.SendTime).ToArray();

            return JsonConvert.SerializeObject(messages);
        }
    }
}
