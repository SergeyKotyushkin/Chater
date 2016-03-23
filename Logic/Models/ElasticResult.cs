namespace Logic.Models
{
    public class ElasticResult
    {
        public ElasticResult(bool success, string message, object value)
        {
            Success = success;
            Message = message;
            Value = value;
        }

        public ElasticResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public ElasticResult(bool success, object value)
        {
            Success = success;
            Value = value;
        }

        public bool Success { get; private set; }

        public string Message { get; private set; }

        public object Value { get; private set; }
    }
}