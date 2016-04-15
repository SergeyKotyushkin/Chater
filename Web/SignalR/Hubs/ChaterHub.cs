using System;
using System.Linq;
using System.Threading.Tasks;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Web.SignalR.Hubs
{
    public class ChaterHub : Hub
    {
        private const string SomeError = "some error#@!";

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
                Clients.Caller.OnLoginCaller(null, null, false, elasticResult.Message ?? "Invalid account data");
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

        // Chat: Create new chat
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

        // Chat: Remove chat
        public void RemoveChat(string guid)
        {
            var elasticResult = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnRemoveChat(false, elasticResult.Message);
                return;
            }

            var user = (User)elasticResult.Value;
            
            elasticResult = _chatUserRepository.GetByChatGuid(guid);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnRemoveChat(false, elasticResult.Message);
                return;
            }

            var chatUsers = ((ChatUser[]) elasticResult.Value);
            var chatUser = chatUsers.FirstOrDefault(cu => cu.UserGuid == user.Guid);
            if (chatUser == null)
            {
                Clients.Caller.OnRemoveChat(false, elasticResult.Message);
                return;
            }

            elasticResult = _chatUserRepository.Remove(chatUser.Guid);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnRemoveChat(false, elasticResult.Message);
                return;
            }

            if (chatUsers.All(cu => cu.Guid != chatUser.Guid))
            {
                elasticResult = _chatRepository.Remove(chatUser.ChatGuid);
                if (!elasticResult.Success)
                {
                    Clients.Caller.OnRemoveChat(false, elasticResult.Message);
                    return;
                }
            }

            Clients.Caller.OnCreateChat(true, null);
        }

        // Chat: Get Users For Chat
        public void GetUsersForChat(string guid)
        {
            var elasticResult = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnGetUsersForChat(false, null, null, elasticResult.Message);
                return;
            }

            var user = (User)elasticResult.Value; 
            
            elasticResult = _chatUserRepository.GetByChatGuid(guid);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnGetUsersForChat(false, null, null, elasticResult.Message);
                return;
            }

            var chatUsers = ((ChatUser[]) elasticResult.Value);
            if (chatUsers == null || chatUsers.Length == 0)
            {
                Clients.Caller.OnGetUsersForChat(false, null, null, elasticResult.Message);
                return;
            }

            elasticResult =
                _userRepository.GetByGuids(chatUsers.Select(cu => cu.UserGuid).Where(g => g != user.Guid).ToArray());
            if (!elasticResult.Success)
            {
                Clients.Caller.OnGetUsersForChat(false, null, null, elasticResult.Message);
                return;
            }

            var usersGuids = ((User[]) elasticResult.Value).Select(u => u.Guid);

            elasticResult = _chatRepository.Get(guid);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnGetUsersForChat(false, null, null, elasticResult.Message);
                return;
            }

            var chat = JsonConvert.SerializeObject((Chat) elasticResult.Value);
            Clients.Caller.OnGetUsersForChat(true, usersGuids, chat, elasticResult.Message);
        }

        // Chat: Update chat
        public void UpdateChat(string guid, string name, string userGuidsString)
        {
            var userGuids = JsonConvert.DeserializeObject<string[]>(userGuidsString);

            var elasticResult = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnUpdateChat(false, null, SomeError);
                return;
            }

            var user = (User) elasticResult.Value;

            var newChat = new Chat(guid, name);
            elasticResult = _chatRepository.Update(newChat);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnUpdateChat(false, null, SomeError);
                return;
            }

            elasticResult = _chatUserRepository.GetByChatGuid(guid);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnUpdateChat(false, null, SomeError);
                return;
            }

            var chatUsers = (ChatUser[]) elasticResult.Value;
            foreach (var chatUser in chatUsers.Where(cu => cu.UserGuid != user.Guid))
            {
                _chatUserRepository.Remove(chatUser.Guid);
            }

            foreach (var userGuid in userGuids)
            {
                _chatUserRepository.Add(guid, userGuid);
            }

            elasticResult =
                _userRepository.GetByGuids(chatUsers.Select(cu => cu.UserGuid).Where(g => g != user.Guid).ToArray());
            if (!elasticResult.Success)
            {
                Clients.Caller.OnUpdateChat(false, null, SomeError);
                return;
            }

            var usersGuids = ((User[])elasticResult.Value).Select(u => u.Guid);

            // TODO: update group

            var chat = JsonConvert.SerializeObject(newChat);
            Clients.Caller.OnUpdateChat(true, chat, "Chat updated");
        }

        // ChatBox: Send Message
        public void SendMessage(string chatGuid, string text)
        {
            var elasticResult = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (!elasticResult.Success || (User)elasticResult.Value == null)
            {
                Clients.Caller.OnSendMessage(false, null, "User not found");
                return;
            }

            var user = (User) elasticResult.Value;

            elasticResult = _chatRepository.Get(chatGuid);
            if (!elasticResult.Success || (Chat)elasticResult.Value == null)
            {
                Clients.Caller.OnSendMessage(false, null, "Chat not found");
                return;
            }
            
            elasticResult = _messageRepository.Add(chatGuid, user.Guid, text);
            if (!elasticResult.Success)
            {
                Clients.Caller.OnSendMessage(false, null, elasticResult.Message);
                return;
            }

            var message = JsonConvert.SerializeObject((Message) elasticResult.Value);
            Clients.All.OnSendMessage(true, message, null);
        }


        // Disconnect
        public void Disconnect()
        {
            var elasticResult = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (!elasticResult.Success)
                return;

            var user = (User) elasticResult.Value;
            // TODO: remove from groups

            if(user == null)
                return;

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