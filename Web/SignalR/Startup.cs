using Logic.ElasticRepository;
using Logic.MessageRepository;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Web.SignalR;
using Web.SignalR.Hubs;

[assembly: OwinStartup(typeof(Startup))]
namespace Web.SignalR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.DependencyResolver.Register(typeof (ChaterHub),
                () => new ChaterHub(
                    new UserRepository(), new ChatRepository(), new ChatUserRepository(), new MessageRepository())
                );
            app.MapSignalR();
        }
    }
}