namespace Logic.Models
{
    public class Chat
    {
        public Chat(string name)
        {
            Guid = System.Guid.NewGuid().ToString();
            Name = name;
        }

        public string Guid { get; set; }

        public string Name { get; set; }
    }
}