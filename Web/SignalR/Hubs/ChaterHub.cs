using System.Linq;
using System.Threading.Tasks;
using Logic.ElasticRepository.Contracts;
using Logic.MessageRepository.Contracts;
using Logic.Models;
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
            var elasticResult = _userRepository.Login(login, password);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnLoginCaller(null, null, false, elasticResult.Message);
                return;
            }

            var user = (User) elasticResult.Value;

            // TODO: update groups
            //UpdateUserChats(user.ConnectionId, Context.ConnectionId);

            user.ConnectionIds.Add(Context.ConnectionId);
            elasticResult = _userRepository.Update(user);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnLoginCaller(null, null, false, elasticResult.Message);
                return;
            }

            elasticResult = _userRepository.GetAll();
            if (!elasticResult.Success)
            {
                Clients.Caller.OnLoginCaller(null, null, false, elasticResult.Message);
                return;
            }

            var users = ((User[]) elasticResult.Value).Where(u => u.Guid != user.Guid);
            var connectionIds = User.GetConnectionIds(users);
            var jsonUsers = JsonConvert.SerializeObject(users);


            elasticResult = _chatUserRepository.GetAllByUserGuid(user.Guid);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnLoginCaller(null, null, false, elasticResult.Message);
                return;
            }

            var chatGuids = ((ChatUser[]) elasticResult.Value).Select(c => c.ChatGuid).ToArray();

            elasticResult = _chatRepository.GetByGuids(chatGuids);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnLoginCaller(null, null, false, elasticResult.Message);
                return;
            }

            var chats = (Chat[]) elasticResult.Value;

            var jsonChats = JsonConvert.SerializeObject(chats);

            Clients.Caller.OnLoginCaller(jsonUsers, jsonChats, true, null);
            Clients.Clients(connectionIds).OnLoginOthers(user.Guid, user.UserName, user.IsOnline);
        }

        //Chat: Create new chat
        public void CreateChat(string name)
        {
            var elasticResult = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnCreateChat(false, null, null, elasticResult.Message);
                return;
            }

            var user = (User) elasticResult.Value;

            elasticResult = _chatRepository.Add(name);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnCreateChat(false, null, null, elasticResult.Message);
                return;
            }

            var chat = (Chat)elasticResult.Value;

            elasticResult = _chatUserRepository.Add(chat.Guid, user.Guid);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnCreateChat(false, null, null, elasticResult.Message);
                return;
            }

            foreach (var connectionId in user.ConnectionIds)
                Groups.Add(connectionId, name);

            Clients.Caller.OnCreateChat(true, name, chat.Guid, null);
        }

        // Disconnect
        public void Disconnect()
        {
            var elasticResult = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (!elasticResult.Success)
                return;

            var user = (User) elasticResult.Value;
            // TODO: remove from groups

            user.ConnectionIds.Remove(Context.ConnectionId);

            elasticResult = _userRepository.Update(user);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnLoginCaller(null, null, false, elasticResult.Message);
                return;
            }

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