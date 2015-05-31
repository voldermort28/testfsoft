using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using MyProject.Functions;

namespace MyProject.Filters
{
    public class SuperAdminAttributes
    {
        public class SuperAdminAttribute : FilterAttribute, IAuthenticationFilter      
        {
            
            public void OnAuthentication(AuthenticationContext context)
            {
                if (!Common.ChekSuperAdmin())
                {
                    context.Result = new HttpUnauthorizedResult(); // mark unauthorized
                }                 
            }

            public void OnAuthenticationChallenge(AuthenticationChallengeContext context)
            {
                if (context.Result == null || context.Result is HttpUnauthorizedResult)
                {                                        
                    context.Result = new RedirectToRouteResult("Default",
                        new System.Web.Routing.RouteValueDictionary { 
                        { "Controller", "LoginCP" },
                        { "Action", "Logout" }, { "returnUrl", context.HttpContext.Request.RawUrl } });
                }
            }
        }
    }
}