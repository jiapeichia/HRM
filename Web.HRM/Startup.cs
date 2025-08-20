using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Meo.Web.Startup))]
namespace Meo.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //createUserandRoles();
        }

        //// In this method we will create default User roles and Admon user for login
        //private void createUserandRoles()
        //{
        //    ApplicationDbContext context = new ApplicationDbContext();

        //    var roleManager = new RoleManager<ApplicationIdentity>(new RoleStore<ApplicationIdentity>(context));
        //    var UserManager = new UserManager<ApplicationIdentity>(new UserStore<ApplicationUser>(context));

        //    // In Startup iam creating  first admin Role nd 

        //    throw new NotImplementedException();
        //}
    }
}
