using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DanceSchool.Startup))]
namespace DanceSchool
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
