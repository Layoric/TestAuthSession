using Funq;
using Microsoft.AspNetCore.Hosting;
using ServiceStack;
using TestAuthSession.ServiceInterface;

[assembly: HostingStartup(typeof(TestAuthSession.AppHost))]

namespace TestAuthSession
{

    public class AppHost : AppHostBase, IHostingStartup
    {
        public void Configure(IWebHostBuilder builder) => builder
            .ConfigureServices(services =>
            {
                // Configure ASP.NET Core IOC Dependencies
            })
            .Configure(app =>
            {
                // Configure ASP.NET Core App
                if (!HasInit)
                    app.UseServiceStack(new AppHost());
            });

        public AppHost() : base("TestAuthSession", typeof(MyServices).Assembly)
        {
        }

        public override void Configure(Container container)
        {
            // Configure ServiceStack only IOC, Config & Plugins
            SetConfig(new HostConfig
            {
                UseSameSiteCookies = true,
            });
        }
    }
}