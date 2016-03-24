using System.Collections.Generic;
using System.Linq;
using Logic.ElasticRepository.Contracts;
using Logic.Models;
using Logic.StructureMap;
using Nest;

namespace Logic.ElasticRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();
        private readonly IEntityRepository _entityRepository = StructureMapFactory.Resolve<IEntityRepository>();

        private const string EsType = "user";

        // Adding new User. Registration.
        public ElasticResult Add(string login, string password, string userName)
        {
            var user = new User(new HashSet<string>(), login, password, userName);
            var response = CheckUser(user);

            return !response.Success ? response : _entityRepository.Add(EsType, user);
        }

        // Updating User
        public ElasticResult Update(User user)
        {
            var response = _elasticRepository.ExecuteCreateOrUpdateRequest(user, EsType);

            return response.Success
                ? new ElasticResult(true, "Success Updating", user)
                : new ElasticResult(false, response.Message);
        }

        // Logging in User
        public ElasticResult Login(string login, string password)
        {
            var searchDescriptor = new SearchDescriptor<User>().Query(
                q =>
                    q.Bool(
                        b =>
                            b.Must(
                                m =>
                                    m.Term(fields => fields.Field(f => f.Login).Value(login)) &&
                                    m.Term(fields => fields.Field(f => f.Password).Value(password)))))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntityIfOnlyOneEntityInElasticResponse<User>(response);
        }

        // Getting User by Guid
        public ElasticResult Get(string guid)
        {
            return _entityRepository.Get<User>(EsType, guid);
        }

        // Getting User by ConnectionId
        public ElasticResult GetByConnectionId(string connectionId)
        {
            var searchDescriptor = new SearchDescriptor<User>().Query(
                q => q.Match(m => m.Field(f => f.ConnectionIds).Query(connectionId))).Index(_elasticRepository.EsIndex).Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntityIfOnlyOneEntityInElasticResponse<User>(response);
        }

        // Getting all Users
        public ElasticResult GetAll()
        {
            return _entityRepository.GetAll<User>(EsType);
        }

        // Getting all Online Users
        public ElasticResult GetAllOnline()
        {
            var searchDescriptor = new SearchDescriptor<User>().Query(
                q => q.Term(t => t.Field(f => f.IsOnline).Value(true))).Index(_elasticRepository.EsIndex).Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntitiesFromElasticResponse<User>(response);
        }

        // Getting Users by Guids
        public ElasticResult GetByGuids(params string[] userGuids)
        {
            return _entityRepository.GetByGuids<User>(EsType, userGuids);
        }

        
        // Check User Is Unique
        private ElasticResult CheckUser(User user)
        {
            var searchDescriptor = new SearchDescriptor<User>().Query(
                q =>
                    q.Bool(
                        b =>
                            b.Should(
                                m => m.Term(t => t.Field(f => f.Login).Value(user.Login)),
                                m => m.Term(t => t.Field(f => f.UserName).Value(user.UserName)))))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            // If request well executed. And user is unique.
            if (response.Success && !((ISearchResponse<User>)response.Response).Hits.Any())
                return new ElasticResult(true, true);

            return response.Success
                ? new ElasticResult(false, "UserName or Login are not unique!")
                : new ElasticResult(false, response.Message);
        }
    }
}