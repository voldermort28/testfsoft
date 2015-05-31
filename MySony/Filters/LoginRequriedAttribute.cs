using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MyProject.Functions;

namespace MyProject.Filters
{
    public class LoginRequriedAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!Common.IsLogedIn())
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {{"Action","Index"},{"Controller","Landing"} });
            }
        }
    }
}