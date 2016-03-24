using System.Linq;
using System.Web.Http;
using Logic.ElasticRepository.Contracts;
using Logic.MessageRepository.Contracts;
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

            elasticResult = _userRepository.GetByGuids(messages.Select(m => m.UserGuid).Distinct().ToArray());
            if (!elasticResult.Success)
                return null;

            var users = (User[]) elasticResult.Value;

            var messageOutput = new MessageOutput[messages.Length];
            for (var i = 0; i < messages.Length; i++)
            {
                var user = users.FirstOrDefault(u => u.Guid == messages[i].UserGuid);
                if (user == null)
                    messageOutput[i] = null;
                else
                    messageOutput[i] = new MessageOutput(user.UserName, messages[i].Text,
                        messages[i].SendTime.ToString("G"));
            }

            return JsonConvert.SerializeObject(messageOutput);
        }
    }
}
