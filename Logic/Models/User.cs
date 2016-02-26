namespace Logic.Models
{
    public class User
    {
        public User(string connectionId, string login, string password, string userName, bool isOnline)
        {
            Guid = System.Guid.NewGuid().ToString();
            ConnectionId = connectionId;
            Login = login;
            Password = password;
            UserName = userName;
            IsOnline = isOnline;
        }

        public string Guid { get; set; }

        public string ConnectionId { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }
 
        public string UserName { get; set; }

        public bool IsOnline { get; set; }
    }
}