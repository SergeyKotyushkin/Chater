namespace Security.Contracts
{
    public interface ISecurity
    {
        bool Login(string login, string password);
    }
}