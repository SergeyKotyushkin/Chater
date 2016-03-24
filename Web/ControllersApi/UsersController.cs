using System.Linq;
using System.Web.Http;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
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
            var elasticResult = _userRepository.GetAll();
            if(!elasticResult.Success)
                return JsonConvert.SerializeObject(new User[] { });

            var users = (User[]) elasticResult.Value;
            return JsonConvert.SerializeObject(users.OrderBy(u => u.UserName));
        }
    }
}
