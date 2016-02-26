using Security.Contracts;

namespace Security
{
    public class Security : ISecurity
    {
        public bool Login(string login, string password)
        {
            return login == "sergey" && password == "123";
        }
    }
}