using System;
using System.Linq;
using System.Web.Mvc;
using MySony.Functions;
using MySony.ViewModels;
using MySony.Models;
namespace MySony.Controllers
{
    //[OutputCache(Duration = 60 * 60, VaryByParam = "none")]
    public class TintucController : Controller
    {
        readonly MySonyEntities _db = new MySonyEntities();
        private const int SizeOfPage = 3;
        // GET: /News/
        //[OutputCache(Duration = 60 * 60, VaryByParam = "none")]
        public ActionResult Index()
        {
            var news = new NewsVM();
            using (var db = new MySonyEntities())
            {
                news.lstNews = (from a in db.RS_Article
                                join c in db.RS_Category on a.CategoryID equals c.ID
                                where c.Tags.Contains(Constant.TagTinTuc  )
                                orderby a.IsTop descending , a.Created descending 
                                select a).Take(3).ToList();

                news.lstPromote = (from a in db.RS_Article
                                join c in db.RS_Category on a.CategoryID equals c.ID
                                   where c.Tags.Contains(Constant.Tagkhuyenmai)
                                orderby a.IsTop descending, a.Created descending 
                                select a).Take(3).ToList();
                ViewBag.MinPage = 1;
                ViewBag.MaxPageNews = (db.RS_Article.Count(x => x.RS_Category.Tags.Contains(Constant.TagTinTuc)) % SizeOfPage == 0) ? db.RS_Article.Count(x => x.RS_Category.Tags.Contains(Constant.TagTinTuc)) / SizeOfPage : db.RS_Article.Count(x => x.RS_Category.Tags.Contains(Constant.TagTinTuc)) / SizeOfPage + 1;
                ViewBag.MaxPageSale = (db.RS_Article.Count(x => x.RS_Category.Tags.Contains(Constant.Tagkhuyenmai)) % SizeOfPage == 0) ? db.RS_Article.Count(x => x.RS_Category.Tags.Contains(Constant.Tagkhuyenmai)) / SizeOfPage : db.RS_Article.Count(x => x.RS_Category.Tags.Contains(Constant.Tagkhuyenmai)) / SizeOfPage + 1;

            }
            return View(news);
        }

        //[OutputCache(Duration = 60 * 60, VaryByParam = "page;tag")]
        public ActionResult GetArticleByPage(int page, String tag)
        {
            if (page < 1)
            {
                return Json(new { result = "False", msg="Không có dữ liệu" },JsonRequestBehavior.AllowGet); 
            }

            var res = (from a in _db.RS_Article
                       join c in _db.RS_Category on a.CategoryID equals c.ID
                       orderby a.IsTop descending  , a.Created descending 
                       where c.Tags.Contains(tag)
                       select new {
                           a.ID,
                           a.Brief,
                           a.PublishDate,
                           a.Slug,
                           a.Thumbnail,
                           a.Title,
                                    })
                       .Skip(3 * (page - 1)).Take(3).ToList();
                if (res.Count > 0)
                {
                     return Json(new { result="OK",msg="",res},JsonRequestBehavior.AllowGet);
                }
            return Json(new { result = "False", msg="Không có dữ liệu" },JsonRequestBehavior.AllowGet);
        }

        //[OutputCache(Duration = 60 * 60, VaryByParam = "category;id;slug")]
        public ActionResult Xem(String category, int id, String slug)
        {
            var art = _db.RS_Article.Find(id);
            if (art == null)
            {
                return Redirect("/NotFoud"); // doi link 404 not found
            }
            var xemvm = new XemTintucVM
            {
                article = art,
                lstSimilar =
                    _db.RS_Article.Where(x => x.CategoryID == art.CategoryID && x.ID != art.ID)
                        .OrderBy(x => x.ID)
                        .Take(10)
                        .ToList()
            };
            return View(xemvm);
        }
    }
}