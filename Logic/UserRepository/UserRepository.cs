using System;
using System.Linq;
using Logic.Models;
using Logic.UserRepository.Contracts;
using Nest;

namespace Logic.UserRepository
{
    public class UserRepository : ElasticRepository.ElasticRepository, IUserRepository
    {
        private const string EsIndex = "database";
        private const string EsType = "user";

        private readonly User[] _users = {};

        public User Add(string connectionId, string login, string password, string userName, bool isOnline)
        {
            try
            {
                var user = new User(connectionId, login, password, userName, isOnline);
                if (!CheckUser(user)) 
                    return null;

                var client = GetElasticClient();
                client.Index(user, i => i.Index(EsIndex).Type(EsType).Id(user.Guid));
                return user;
            }
            catch
            {
                return null;
            }
        }

        public User Update(User user)
        {
            try
            {
                var client = GetElasticClient();
                client.Index(user, i => i.Index(EsIndex).Type(EsType).Id(user.Guid));
                return user;
            }
            catch
            {
                return null;
            }
        }

        public User Login(string login, string password)
        {
            try
            {
                var client = GetElasticClient();
                var hits =
                    client.Search<User>(
                        s => s.Query(q => q.Match(m => m.Field("login").Query(login)) &&
                                          q.Match(m => m.Field("password").Query(password)) &&
                                          q.Match(m => m.Field("isOnline").Query("false")))
                              .Index(EsIndex).Type(EsType)).Hits;

                var result = hits as IHit<User>[] ?? hits.ToArray();
                return result.Count() == 1 ? result[0].Source : null;
            }
            catch
            {
                return null;
            }
        }

        // no use
        public User Get(string guid)
        {
            try
            {
                var client = GetElasticClient();
                var hits = client.Search<User>(s => s.Query(q => q.Match(m => m.Field("guid").Query(guid)))
                                                     .Index(EsIndex).Type(EsType)).Hits;

                var result = hits as IHit<User>[] ?? hits.ToArray();
                return result.Count() == 1? result[0].Source: null;
            }
            catch
            {
                return null;
            }
        }

        public User GetByConnectionId(string connectionId)
        {
            try
            {
                var client = GetElasticClient();
                var hits =
                    client.Search<User>(
                        s => s.Query(q => q.Match(m => m.Field("connectionId").Query(connectionId)))
                              .Index(EsIndex).Type(EsType)).Hits;

                var result = hits as IHit<User>[] ?? hits.ToArray();
                return result.Count() == 1 ? result[0].Source : null;
            }
            catch
            {
                return null;
            }
        }

        public User[] GetAll()
        {
            try
            {
                var client = GetElasticClient();
                var hits =
                    client.Search<User>(s => s.Index(EsIndex).Type(EsType)).Hits;

                return hits.Select(hit => hit.Source).Where(s => s != null).ToArray();
            }
            catch
            {
                return null;
            }
        }

        public User[] GetAllOnline()
        {
            try
            {
                var client = GetElasticClient();
                var hits = client.Search<User>(s => s.Query(q => q.Match(m => m.Field("isOnline").Query("true")))
                                                     .Index(EsIndex).Type(EsType)).Hits;

                return hits.Select(hit => hit.Source).ToArray();
            }
            catch
            {
                return null;
            }
        }

        public User[] GetByGuids(params string[] userGuids)
        {
            try
            {
                var client = GetElasticClient();
                var result = client.MultiGet(m => m.GetMany<User>(userGuids).Index(EsIndex).Type(EsType));
                var hits = result.GetMany<User>(userGuids);

                return hits.Select(hit => hit.Source).Where(hit => hit != null).ToArray();
            }
            catch
            {
                return null;
            }
        }


        private bool CheckUser(User user)
        {
            try
            {
                var client = GetElasticClient();
                var hitsCount =
                    client.Search<User>(s => s.Query(q => q.Match(m => m.Field("login").Query(user.Login)))
                                              .Index(EsIndex).Type(EsType)).Hits.Count();

                if (hitsCount != 0) return false;

                hitsCount =
                    client.Search<User>(s => s.Query(q => q.Match(m => m.Field("username").Query(user.UserName)))
                                              .Index(EsIndex).Type(EsType)).Hits.Count();
                return hitsCount == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}