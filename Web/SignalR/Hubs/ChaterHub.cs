using System.Linq;
using System.Threading.Tasks;
using Logic.ChatRepository.Contracts;
using Logic.ChatUserRepository.Contracts;
using Logic.MessageRepository.Contracts;
using Logic.UserRepository.Contracts;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Web.SignalR.Hubs
{
    public class ChaterHub : Hub
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IChatUserRepository _chatUserRepository;
        private readonly IMessageRepository _messageRepository;

        public ChaterHub(IUserRepository userRepository, IChatRepository chatRepository,
            IChatUserRepository chatUserRepository, IMessageRepository messageRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _chatUserRepository = chatUserRepository;
            _messageRepository = messageRepository;
        }


        // Login User
        public void Login(string login, string password)
        {
            var user = _userRepository.Login(login, password);
            if (user == null)
            {
                Clients.Caller.OnLoginCaller(null, false, "Connection error or invalid user info");
                return;
            }

            //UpdateUserChats(user.ConnectionId, Context.ConnectionId);

            user.ConnectionId = Context.ConnectionId;
            user.IsOnline = true;
            user = _userRepository.Update(user);
            if (user == null)
            {
                Clients.Caller.OnLoginCaller(null, false, "Connection error");
                return;
            }

            var users = _userRepository.GetAll().Where(u => u.Guid != user.Guid).ToArray();
            var jsonUsers = JsonConvert.SerializeObject(users);
            Clients.Caller.OnLoginCaller(jsonUsers, true, string.Empty);
            Clients.Others.OnLoginOthers(user.Guid, user.UserName, user.IsOnline);
        }



        // Disconnect
        public void Disconnect()
        {
            var user = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (user == null) return;

            user.IsOnline = false;
            user = _userRepository.Update(user);
            Clients.Others.OnDisconnectOthers(user.Guid);
            Clients.Caller.OnDisconnectCaller(user.UserName);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Disconnect();

            return base.OnDisconnected(stopCalled);
        }
    }
}