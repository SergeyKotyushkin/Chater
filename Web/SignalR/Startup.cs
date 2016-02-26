﻿using Logic.ChatRepository;
using Logic.ChatUserRepository;
using Logic.UserRepository;
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
            GlobalHost.DependencyResolver.Register(typeof (ChatHub),
                () => new ChatHub(new UserRepository(), new ChatRepository(), new ChatUserRepository()));
            app.MapSignalR();
        }
    }
}