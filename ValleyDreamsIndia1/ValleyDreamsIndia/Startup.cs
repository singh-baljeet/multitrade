using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ValleyDreamsIndia.Startup))]
namespace ValleyDreamsIndia
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
