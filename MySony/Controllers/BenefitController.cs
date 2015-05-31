using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySony.Controllers
{
    [OutputCache(Duration=60*60,VaryByParam="none")]
    public class BenefitController : Controller
    {
        // GET: Benefit
        public ActionResult Index()
        {
            return View();
        }
    }
}