using System;
using System.Linq;
using System.Threading.Tasks;
using Logic.ChatRepository.Contracts;
using Logic.ChatUserRepository.Contracts;
using Logic.MessageRepository.Contracts;
using Logic.Models;
using Logic.UserRepository.Contracts;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Newtonsoft.Json;

namespace Web.SignalR.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IChatUserRepository _chatUserRepository;
        private readonly IMessageRepository _messageRepository;

        public ChatHub(IUserRepository userRepository, IChatRepository chatRepository,
            IChatUserRepository chatUserRepository, IMessageRepository messageRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _chatUserRepository = chatUserRepository;
            _messageRepository = messageRepository;
        }

        public void Send(string chatGuid, string text)
        {
            var user = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (user == null) return;

            var messageOutput = new MessageOutput(user.UserName, text, DateTime.Now.ToString("G"));
            var message = _messageRepository.Add(chatGuid, user.Guid, text);
            if (message == null)
                Clients.Caller.OnSendResult(messageOutput, chatGuid, false);
            else
                Clients.Group(chatGuid).OnSendResult(messageOutput, chatGuid, true);
        }

        // Login User
        public void Login(string login, string password)
        {
            var user = _userRepository.Login(login, password);
            if (user == null)
            {
                Clients.Caller.OnLoginResult(null, null, false, "Some error occured");
                return;
            }

            UpdateUserChats(user.ConnectionId, Context.ConnectionId);

            user.ConnectionId = Context.ConnectionId;
            user.IsOnline = true;
            user = _userRepository.Update(user);
            if (user == null)
            {
                Clients.Caller.OnLoginResult(null, null, false, "Some error occured");
                return;
            }

            var onlineUsers = _userRepository.GetAllOnline().Where(u => u.Guid != user.Guid).ToArray();
            var jsonUsers = JsonConvert.SerializeObject(onlineUsers);
            var jsonUser = JsonConvert.SerializeObject(user);
            Clients.Caller.OnLoginResult(jsonUser, jsonUsers, true, string.Empty);
            Clients.Others.OnUserLoggedInResult(user.UserName);
        }

        // Registration User
        public void Register(string login, string password, string userName)
        {
            // skip check retry

            var user = _userRepository.Add(Context.ConnectionId, login, password, userName, false);

            Clients.Caller.OnRegisterResult(user != null, string.Empty);
        }

        // Menu -> New Chat:
        // Create New Chat By Name
        public void CreateChat(string name)
        {
            var user = _userRepository.GetByConnectionId(Context.ConnectionId);
            if (user == null) return;

            var chat = _chatRepository.Add(name);
            if(chat == null) return;

            var chatUser = _chatUserRepository.Add(chat.Guid, user.Guid);
            if (chatUser == null)
            {
                _chatRepository.Remove(chat.Guid);
                return;
            }

            Groups.Add(Context.ConnectionId, name);
            Clients.Caller.OnCreateChatResult(true, name, chat.Guid, string.Empty);
        }

        public void UsersAddToChat(string chatGuid, string usersGuids)
        {
            var usersGuidsArray = usersGuids.Split(',');

            for (var i = 0; i < usersGuidsArray.Count(); i++)
            {
                var user = _userRepository.Get(usersGuidsArray[i]);
                if (user == null) continue;

                var result = _chatUserRepository.Add(chatGuid, usersGuidsArray[i]);
                if(result == null) continue;

                Groups.Add(user.ConnectionId, chatGuid);
            }

            Clients.Caller.onUsersAddToChatResult();
        }

        // Menu -> Exit
        public void Disconnect()
        {
            var user = _userRepository.GetByConnectionId(Context.ConnectionId);
            //user = _userRepository.GetByConnectionId("78269e3c-517b-4d5b-a0cb-0811a4d01f5a");
            //user = _userRepository.GetByConnectionId("5e121230-a4a2-414b-8fa9-32c3a10a6c6b");
            if(user == null) return;

            user.IsOnline = false;
            user = _userRepository.Update(user);
            Clients.Others.OnDisconnectResult(user.UserName);
            Clients.Caller.OnDisconnectCallerResult(user.UserName);
        }

        // Отключение пользователя
        public override Task OnDisconnected(bool stopCalled)
        {
            Disconnect();

            return base.OnDisconnected(stopCalled);
        }


        private void UpdateUserChats(string oldConnectionId, string newConnectionId)
        {
            var user = _userRepository.GetByConnectionId(oldConnectionId);
            if (user == null) return;

            var chats = _chatUserRepository.GetAllForUser(user.Guid).Select(cu => cu.ChatGuid).Distinct();
            foreach (var chat in chats)
            {
                Groups.Remove(oldConnectionId, chat);
                Groups.Add(newConnectionId, chat);
            }
        }
    }
}