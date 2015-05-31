using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySony.Functions;
using MySony.Models;
using MySony.ViewModels;

namespace MySony.Controllers
{
    //[OutputCache(Duration = 60*5, VaryByParam = "none",Location=System.Web.UI.OutputCacheLocation.Client)]
   // [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (!Common.IsLogedIn())
            {
                return Redirect("/Landing");
            }
            var viewObj = new NewsVM();
            // tin tuc , khuyen mai
            List<NewsVM_001> lstObj1 = null;
            // Qua Tang
            List<NewsVM_001> lstQuaTang = null;
            //khoa huan luyen
            // video gioi thieu san phan //cuoc thi giai tri
            List<RS_Video> lstgame = null;

            List<NewsVM_001> lstTraning = null; ;
            List<NewsVM_001> lstImageHome = null; ;
            try
            {
               
                using (var db = new MySonyEntities())
                {
                    var states = new List<string>() { Constant.TagTinTuc, Constant.Tagkhuyenmai };
                    lstObj1 = (from a in db.RS_Article
                               join c in db.RS_Category on a.CategoryID equals c.ID
                               where states.Contains(c.Tags)
                               orderby a.IsTop descending, a.Created descending
                               select new NewsVM_001
                               {
                                   Thumbnail = a.Thumbnail,
                                   Title = a.Title,
                                   Date = a.Created != null ? (DateTime)a.Created : DateTime.Now,
                                   ArticleContent = a.ArticleContent,
                                   ID = a.ID,
                                   Slug = a.Slug
                               }).Take(4).ToList();

                    lstQuaTang = (from a in db.RS_Article
                                  join c in db.RS_Category on a.CategoryID equals c.ID
                                  where c.Tags.Contains(Constant.TagQuaTang)
                                  orderby a.IsTop descending, a.Created descending
                                  select new NewsVM_001
                                  {
                                      Thumbnail = a.Thumbnail,
                                      Title = a.Title,
                                      Date = a.Created != null?  (DateTime)a.Created : DateTime.Now,
                                      ArticleContent = a.ArticleContent,
                                      ID = a.ID,
                                      Slug = a.Slug
                                  }).Take(4).ToList();

                    lstgame = db.RS_Video.Where(x => x.IsActive).Take(4).ToList();

                    var id = db.RS_Category.FirstOrDefault(x => x.IsActive && x.Tags.Contains(Constant.TagHuanLuyen));
                    if (id != null)
                    {
                    }

                    lstTraning = (from c in db.RS_Category
                                  join a in db.RS_Images on c.ID equals a.CategoryID
                                  where (c.Tags.Contains(Constant.TagHuanLuyen) && c.IsActive && a.IsActive)
                                  orderby a.Modified descending, a.Created descending
                                  select new NewsVM_001
                                  {
                                      Thumbnail = a.LinkImage,
                                      Title = a.Title,
                                      Date = a.Created != null ? (DateTime)a.Created : DateTime.Now,
                                      ID = (int)a.CategoryID
                                  }).Take(4).ToList();

                    lstImageHome = (from c in db.RS_Category
                                    join a in db.RS_Images on c.ID equals a.CategoryID
                                    where c.Tags.Contains(Constant.TagImageHome) && a.IsActive
                                    orderby a.Modified descending, a.Created descending
                                    select new NewsVM_001
                                    {
                                        Thumbnail = a.LinkImage,
                                        Title = a.Title,
                                        Date = DateTime.Now,
                                        ID = (int)a.CategoryID ,
                                        Url =  a.Url
                                    }).Take(4).ToList();


                }
               
            }
            catch (Exception)
            {                               
            }
            viewObj.lstCategory = lstObj1;
            viewObj.lstQuaTang = lstQuaTang;
            viewObj.lstGame = lstgame;
            viewObj.lstTraning = lstTraning;
            viewObj.lstImageHome = lstImageHome;
            return View(viewObj);
        }


        public ActionResult GetImformation()
        {
            List<RS_Article> lstObj;
            using (var db = new MySonyEntities())
            {
                var states = new List<string> { Constant.TagTinTuc, Constant.Tagkhuyenmai };
                lstObj = (from a in db.RS_Article
                          join c in db.RS_Category on a.CategoryID equals c.ID
                          where states.Contains(c.Tags)
                          orderby a.IsTop descending, a.Created descending
                          select new RS_Article
                          {
                              Thumbnail = a.Thumbnail,
                              Title = a.Title,
                              ArticleContent = a.ArticleContent
                          }).ToList();
            }
            return Json(new { result = "OK", lstObj }, JsonRequestBehavior.AllowGet);
        }
    }


}