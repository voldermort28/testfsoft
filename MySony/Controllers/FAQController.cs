using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySony.Models;
using MySony.ViewModels;

namespace MySony.Controllers
{
     //[OutputCache(Duration = 60 * 60, VaryByParam = "none")]
    public class FAQController : Controller
    {
        MySonyEntities db = new MySonyEntities();
        private const int SizeOfPage = 6;
        // GET: FAQ              
                
        public ActionResult Index()
        {
            var news = new FAQVM();
            news.LstFaq = (db.RS_FAQ).Take(SizeOfPage).ToList();
            ViewBag.MinPage = 1;
            ViewBag.MaxPageNews = (db.RS_FAQ.ToList().Count() % SizeOfPage == 0) ? db.RS_FAQ.ToList().Count() / SizeOfPage : (int)(db.RS_FAQ.ToList().Count() / SizeOfPage) + 1;
            return View(news);
        }

        public ActionResult GetFAQByPage(int page)
        {
            if (page < 1)
            {
                return Json(new { result = "False", msg = "Không có dữ liệu" }, JsonRequestBehavior.AllowGet);
            }

            var res = (from a in db.RS_FAQ
                       orderby a.ID
                       select new
                       {
                           a.ID,
                           a.Question,
                           a.Answer,
                           a.Status 
                       })
                       .Skip(SizeOfPage * (page - 1)).Take(SizeOfPage).ToList();
                     
            if (res.Count > 0)
            {
                return Json(new { result = "OK", msg = "", res }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "False", msg = "Không có dữ liệu" }, JsonRequestBehavior.AllowGet);
            }
        }

       
    }
}