using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InterestApp.Startup))]
namespace InterestApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
