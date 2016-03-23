using Nest;

namespace Logic.Models
{
    public class ElasticResponse
    {
        public ElasticResponse(bool success, string message, object response)
        {
            Success = success;
            Message = message;
            Response = response;
        }

        public ElasticResponse(bool success, object response)
        {
            Success = success;
            Response = response;
        }

        public ElasticResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; private set; }

        public string Message { get; private set; }

        public object Response { get; private set; }
    }
}