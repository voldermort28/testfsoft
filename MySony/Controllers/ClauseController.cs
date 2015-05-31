using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySony.Controllers
{
     [OutputCache(Duration = 60 * 60, VaryByParam = "none")]
    public class ClauseController : Controller
    {
        //
        // GET: /Clause/
        public ActionResult Index()
        {
            return View();
        }
	}
}