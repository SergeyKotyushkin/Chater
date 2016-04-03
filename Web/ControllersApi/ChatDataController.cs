using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Newtonsoft.Json;

namespace Web.ControllersApi
{
    public class ChatDataController : ApiController
    {
        private readonly IChatUserRepository _chatUserRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;

        public ChatDataController(IChatUserRepository chatUserRepository, IChatRepository chatRepository,
            IUserRepository userRepository)
        {
            _chatUserRepository = chatUserRepository;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }

        // GET api/chatdata/id
        public string Get(string id)
        {
            var elasticResult = _chatRepository.Get(id);
            if (!elasticResult.Success)
            {
                return JsonConvert.SerializeObject(new {Result = false, elasticResult.Message});
            }

            // TODO: get messages
            return JsonConvert.SerializeObject(new { Result = true, Chat = (Chat)elasticResult.Value });
        }
    }
}
