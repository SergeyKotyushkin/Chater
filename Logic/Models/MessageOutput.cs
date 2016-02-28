namespace Logic.Models
{
    public class MessageOutput
    {
        public MessageOutput(string userName, string text, string sendTime)
        {
            UserName = userName;
            Text = text;
            SendTime = sendTime;
        }

        public string UserName { get; set; }

        public string Text { get; set; }

        public string SendTime { get; set; }
    }
}