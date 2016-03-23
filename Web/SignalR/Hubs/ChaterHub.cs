using System.Linq;
using System.Threading.Tasks;
using Logic.ChatRepository.Contracts;
using Logic.ChatUserRepository.Contracts;
using Logic.MessageRepository.Contracts;
using Logic.Models;
using Logic.UserRepository.Contracts;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Web.SignalR.Hubs
{
    public class ChaterHub : Hub
    {
        private const string ConnectionErrorMessage = "Connection error";

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

        // Registration: Register User
        public void Register(string login, string password, string userName)
        {
            var addResult = _userRepository.Add(login, password, userName);

            var result = addResult.Success;
            Clients.Caller.OnRegister(result, addResult.Message);
        }

        // Login: User
        public void Login(string login, string password)
        {
            var user = _userRepository.Login(login, password);
            if (user == null)
            {
                Clients.Caller.OnLoginCaller(null, null, false, "Connection error or invalid user info");
                return;
            }

            // TODO: update groups
            //UpdateUserChats(user.ConnectionId, Context.ConnectionId);

            user.ConnectionIds.Add(Context.ConnectionId);
            user = _userRepository.Update(user);
            if (user == null)
            {
                Clients.Caller.OnLoginCaller(null, null, false, ConnectionErrorMessage);
                return;
            }

            var users = _userRepository.GetAll().Where(u => u.Guid != user.Guid);
            var connectionIds = User.GetConnectionIds(users);
            var jsonUsers = JsonConvert.SerializeObject(users);

            var chatGuids = _chatUserRepository.GetAllByUserGuid(user.Guid).Select(c => c.ChatGuid).ToArray();
            var chats = _chatRepository.GetByIds(chatGuids);
            var jsonChats = JsonConvert.SerializeObject(chats);

            Clients.Caller.OnLoginCaller(jsonUsers, jsonChats, true, null);
            Clients.Clients(connectionIds).OnLoginOthers(user.Guid, user.UserName, user.IsOnline);
        }

        //Chat: Create new chat
        public void CreateChat(string name)
        {
            var user = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (user == null)
            {
                Clients.Caller.OnCreateChat(false, null, null, ConnectionErrorMessage);
                return;
            }

            var chat = _chatRepository.Add(name);
            if (chat == null)
            {
                Clients.Caller.OnCreateChat(false, null, null, ConnectionErrorMessage);
                return;
            }

            var chatUser = _chatUserRepository.Add(chat.Guid, user.Guid);
            if (chatUser == null)
            {
                _chatRepository.Remove(chat.Guid);
                Clients.Caller.OnCreateChat(false, null, null, ConnectionErrorMessage);
                return;
            }

            foreach (var connectionId in user.ConnectionIds)
                Groups.Add(connectionId, name);

            Clients.Caller.OnCreateChat(true, name, chat.Guid, null);
        }

        // Disconnect
        public void Disconnect()
        {
            var user = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (user == null) return;


            // TODO: remove from groups

            user.ConnectionIds.Remove(Context.ConnectionId);
            user = _userRepository.Update(user);
            if (!user.IsOnline)
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