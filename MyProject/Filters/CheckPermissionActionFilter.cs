using System.Web.Mvc;
using System.Web.Routing;

namespace MyProject.Filters
{
    public class CheckPermissionActionFilter : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {
            // Check Permission Action Filter Call
            if (context.HttpContext.Session["admss"] == null)
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "Controller", "LoginCP" }, { "Action", "Index" }, { "returnUrl", context.HttpContext.Request.RawUrl } });
        }
    }
}