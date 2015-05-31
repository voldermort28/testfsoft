using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySony.Models;
using MySony.Filters;

namespace MySony.Controllers
{
     //[OutputCache(Duration = 60 * 60, VaryByParam = "none")]
    public class GameController : Controller
    {
        MySonyEntities db = new MySonyEntities();
        // GET: Game
        [LoginRequried]
        public ActionResult Index()
        {
            var res = db.RS_Video.Where(x => x.IsActive == true).OrderByDescending(x=>x.IsHighlight).ToList();
            return View(res);
        }
    }
}