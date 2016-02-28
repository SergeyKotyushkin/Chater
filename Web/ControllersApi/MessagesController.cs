using System.Linq;
using System.Web.Http;
using Logic.ChatRepository.Contracts;
using Logic.MessageRepository.Contracts;
using Logic.Models;
using Logic.UserRepository.Contracts;
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
            var chat = _chatRepository.Get(id);
            if (chat == null)
                return null;

            var messages = _messageRepository.GetByChatId(id).OrderBy(m => m.SendTime).ToArray();
            if (messages.Length == 0)
                return null;

            var users = _userRepository.GetByGuids(messages.Select(m => m.UserGuid).Distinct().ToArray());
            if (users == null)
                return null;

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
