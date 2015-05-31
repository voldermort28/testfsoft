using MyProject.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MyProject
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
        }

        protected void Application_PreRequestHandlerExecute(Object sender, EventArgs e)
        {
            if (HttpContext.Current.Session != null)
            {
                if (Session["UserID"] != null)
                {
                    string cacheKey = Session["UserID"].ToString();
                    if ((string)HttpContext.Current.Cache[cacheKey] != Session.SessionID)
                    {
                        //Common.SetUserLoggout();
                        Response.RedirectToRoute(new RouteValueDictionary { { "Controller", "UserCP" }, { "Action", "Logout" }, { "next",HttpContext.Current.Request.RawUrl } });
                    }

                    string user = (string)HttpContext.Current.Cache[cacheKey];
                }
            }
        }
    }
}
