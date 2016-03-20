using System.Collections.Generic;
using System.Linq;
using Logic.Models;
using Logic.UserRepository.Contracts;
using Nest;

namespace Logic.UserRepository
{
    public class UserRepository : ElasticRepository.ElasticRepository, IUserRepository
    {
        private const string EsType = "user";

        public static string EsIndex
        {
            get { return "database"; }
        }


        public User Add(string login, string password, string userName)
        {
            try
            {
                var user = new User(new HashSet<string>(), login, password, userName);
                if (!CheckUser(user)) 
                    return null;

                CreateIndex<User>(EsIndex);

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
                        s =>
                            s.Query(
                                q =>
                                    q.Bool(
                                        b =>
                                            b.Must(
                                                m =>
                                                    m.Term(fields => fields.Field(f => f.Login).Value(login)) &&
                                                    m.Term(fields => fields.Field(f => f.Password).Value(password)))))
                                .Index(EsIndex)
                                .Type(EsType)).Hits;

                var result = hits as IHit<User>[] ?? hits.ToArray();
                return result.Count() == 1 ? result[0].Source : null;
            }
            catch
            {
                return null;
            }
        }

        public User Get(string guid)
        {
            try
            {
                var client = GetElasticClient();
                var hits = client.Search<User>(
                    s =>
                        s.Query(
                            q =>
                                q.Bool(
                                    b =>
                                        b.Must(
                                            m =>
                                                m.Term(fields => fields.Field(f => f.Guid).Value(guid)))))
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
                        s =>
                            s.Query(q => q.Match(m => m.Field(f => f.ConnectionIds).Query(connectionId)))
                                .Index(EsIndex)
                                .Type(EsType)).Hits;

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
                var hits = client.Search<User>(s => s.AllIndices().Index(EsIndex).Type(EsType)).Hits;

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
                var hits = client.Search<User>(
                        s =>
                            s.Query(
                                q =>
                                    q.Bool(
                                        b =>
                                            b.Must(
                                                m =>
                                                    m.Term(fields => fields.Field(f => f.IsOnline).Value(true)))))
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
                    client.Search<User>(
                        s =>
                            s.Query(
                                q =>
                                    q.Bool(
                                        b =>
                                            b.Should(
                                                m => m.Term(t => t.Field(f => f.Login).Value(user.Login)),
                                                m => m.Term(t => t.Field(f => f.UserName).Value(user.UserName)))))
                                .Index(EsIndex)
                                .Type(EsType)).Hits.Count();

                return hitsCount == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}