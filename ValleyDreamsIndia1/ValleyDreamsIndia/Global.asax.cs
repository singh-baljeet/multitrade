using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using ValleyDreamsIndia.Common;
using ValleyDreamsIndia.Controllers.Members;

namespace ValleyDreamsIndia
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        //{
            
        //    var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
        //    if (authCookie != null)
        //    {
        //        FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
        //         CurrentUser.CurrentUserId =  Convert.ToInt32(authTicket.UserData);
        //        if (authTicket != null && !authTicket.Expired)
        //        {
        //            var roles = authTicket.UserData.Split(',');
        //            HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(authTicket), roles);

        //        }


        //    }
        //}

        //protected void Application_Error(object sender , EventArgs e)
        //{
        //    Exception exception = Server.GetLastError();
        //    if (exception is CryptographicException)
        //    {
        //        Response.Cookies[".ASPXAUTH"].Expires = DateTime.Now.AddDays(-1);
        //        FormsAuthentication.SignOut();
        //        Response.Redirect("/Home/Index");

        //    }
        //}

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }
    }
}
