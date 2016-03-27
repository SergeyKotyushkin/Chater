using System.Net.Http;
using System.Web.Http;
using Logic.ElasticRepository.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Web.ControllersApi
{
    public class ChatUsersController : ApiController
    {
        private readonly IChatUserRepository _chatUserRepository;

        public ChatUsersController(IChatUserRepository chatUserRepository)
        {
            _chatUserRepository = chatUserRepository;
        }

        // GET api/chatusers/5
        public string Get(string id)
        {
            var chatUsers = _chatUserRepository.GetByChatGuid(id);
            
            return JsonConvert.SerializeObject(chatUsers);
        }

        // Post api/chatusers/
        //public async void Post()
        //{
        //    dynamic obj = await Request.Content.ReadAsAsync<JObject>();
        //    var chatGuid = ((dynamic) ((JObject) (obj))).val.chatGuid.Value;
        //    var count = ((dynamic)((JObject)(obj))).val.usersGuids.Count;
        //    for (var i = 0; i < count; i++)
        //       _chatUserRepository.Add(chatGuid, ((dynamic) ((JObject) (obj))).val.usersGuids[i].Value);
        //}
    }
}
