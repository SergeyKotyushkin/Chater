using Logic.UserRepository.Contracts;
using Security.Contracts;
using StructureMap;
using StructureMap.Graph;

namespace Logic.StructureMap
{
    public class AllRegistry : Registry
    {
        public AllRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            For<ISecurity>().Use<Security.Security>();
            For<IUserRepository>().Use<UserRepository.UserRepository>();
        }
    }
}