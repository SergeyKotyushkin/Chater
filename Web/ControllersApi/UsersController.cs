using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Logic.ElasticRepository.Contracts;
using Newtonsoft.Json;

namespace Web.ControllersApi
{
    public class UsersController : ApiController
    {
        private readonly IUserRepository _userRepository;
        
        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET api/users
        public string Get()
        {
            var users = _userRepository.GetAll();

            return JsonConvert.SerializeObject(users.OrderBy(u => u.UserName));
        }
    }
}
